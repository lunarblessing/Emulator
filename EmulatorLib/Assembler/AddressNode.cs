namespace Emulator.Assembler
{
	public class AddressNode : SyntaxNode
	{
		const int StartState = 0; // Waiting [
		const int FirstRegExpected = 1; // Waiting reg1
		const int OperatorOrBracketExpected = 2; // after reg1 or reg2
		const int Reg2OrConstExpected = 3; // After Reg1+
		const int ScaleExpected = 4; //after reg*
		const int AddOrBracketExpected = 5; // After reg2*scale
		const int ConstExpected = 6; //After reg2*scale+
		const int BracketExpected = 7; // Closing ] exptected
		const int Finished = 8;

		public uint? FirstReg { get; set; }
		public uint? SecondReg { get; set; }
		public SimpleValueNode Scale { get; set; }
		public SimpleValueNode Const { get; set; }

		public AddressNode()
		{
			State = StartState;
		}

		protected override (bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode) ParseToken( Token token )
		{
			if (State == StartState)
			{
				if (token.Value == "[")
				{
					State = FirstRegExpected;
					return WaitingNextToken;
				}
				else
				{
					Error = token.Line + "[ expected";
					return ErrorTuple;
				}
			}
			else if (State == FirstRegExpected)
			{
				if (token.Type == TokenType.Register)
				{
					if (CPU.Is32bitRegister( token.Value ))
					{
						FirstReg = CPU.GetRegisterIndex( token.Value );
						State = OperatorOrBracketExpected;
						return WaitingNextToken;
					}
					else
					{
						Error = token.Line + "32-bit register expected";
						return ErrorTuple;
					}
				}
				else
				{
					Error = token.Line + "Register expected";
					return ErrorTuple;
				}
			}
			else if (State == OperatorOrBracketExpected)
			{
				if (token.Value == "+")
				{
					if (SecondReg == null)
					{
						State = Reg2OrConstExpected;
					}
					else
					{
						State = ConstExpected;
					}
					return WaitingNextToken;
				}
				else if (token.Value == "-")
				{
					State = BracketExpected;
					Const = new SimpleValueNode();
					return ChildWithObligatory( Const );
				}
				else if (token.Value == "*")
				{
					State = ScaleExpected;
					return WaitingNextToken;
				}
				else if (token.Value == "]")
				{
					State = Finished;
					return FinishedAndAdd;
				}
				else
				{
					Error = token.Line + "Operator or ] expected";
					return ErrorTuple;
				}
			}
			else if (State == Reg2OrConstExpected)
			{
				if (token.Type == TokenType.Register)
				{
					if (CPU.Is32bitRegister( token.Value ))
					{
						SecondReg = CPU.GetRegisterIndex( token.Value );
						State = OperatorOrBracketExpected;
						return WaitingNextToken;
					}
					else
					{
						Error = token.Line + "32-bit register expected";
						return ErrorTuple;
					}
				}
				else
				{
					Const = new SimpleValueNode();
					State = BracketExpected;
					return ChildWithObligatory( Const );
				}
			}
			else if (State == ScaleExpected)
			{
				Scale = new SimpleValueNode();
				State = AddOrBracketExpected;
				return ChildWithObligatory( Scale );
			}
			else if (State == AddOrBracketExpected)
			{
				if (token.Value == "]")
				{
					State = Finished;
					return FinishedAndAdd;
				}
				else if (token.Value == "+")
				{
					State = ConstExpected;
					return WaitingNextToken;
				}
				else if (token.Value == "-")
				{
					State = BracketExpected;
					Const = new SimpleValueNode();
					return ChildWithObligatory( Const );
				}
				else
				{
					Error = token.Line + "+ or - or ] expected";
					return ErrorTuple;
				}
			}
			else if (State == ConstExpected)
			{
				Const = new SimpleValueNode();
				State = BracketExpected;
				return ChildWithObligatory( Const );
			}
			else if (State == BracketExpected)
			{
				if (token.Value == "]")
				{
					State = Finished;
					return FinishedAndAdd;
				}
				else
				{
					Error = token.Line + "] expected";
					return ErrorTuple;
				}
			}
			else
			{
				Error = token.Line + "Address expected to finish";
				return ErrorTuple;
			}
		}

		protected override ParsingError? OnParsingEnded()
		{
			if (Scale != null && Scale.IsLiteral && (Scale.Literal > 128 || Scale.Literal <= 0 || (Scale.Literal & (Scale.Literal - 1)) != 0))
			{
				Error = "Scale should be a power of 2 in [1:128] range";
				return Error;
			}
			if (Const != null && Const.IsLiteral && (Const.Literal < sbyte.MinValue || Const.Literal > sbyte.MaxValue))
			{
				Error = "Const address should be in [-128; 127] range";
				return Error;
			}
			return null;
		}
	}
}
