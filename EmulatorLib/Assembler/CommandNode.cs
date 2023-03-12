namespace Emulator.Assembler
{
	public class CommandNode : SyntaxNode
	{
		const int StartState = 0;
		const int WaitingFirstOpOrSize = 1;
		const int WaitingFirstOpOrEnd = 2;
		const int WaitingFirstOp = 3;
		const int WaitingCommaOrEnd = 4;
		const int WaitingComma = 5;
		const int WaitingSecondOp = 6;
		const int Finished = 10;

		public bool IsLabel { get; set; }
		public string LabelName { get; set; }
		public bool IsActualCommand { get; set; }
		public OperandsMode? OperandsMode { get; set; }
		public Command ActualCommand { get; set; }
		public SizeMode? SizeMode { get; set; }
		public Operand? Op1 { get; set; }
		public Operand? Op2 { get; set; }

		(Command? noOp, Command? oneOp, Command? twoOp) _overrides;

		protected override (bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode) ParseToken( Token token )
		{
			if (State == StartState)
			{
				if (token.Type == TokenType.Label)
				{
					IsLabel = true;
					LabelName = token.Value.Replace(":", "");
					State = Finished;
					return FinishedAndAdd;
				}
				else if (token.Type == TokenType.Identifier)
				{
					if (Command.CommandCodeExists( token.Value ))
					{
						IsActualCommand = true;
						_overrides = Command.GetOverridesOfCommand( token.Value );
						return UpdateAfterCommandCode();
					}
					else
					{
						Error = token.Line + "Unknown command";
						return ErrorTuple;
					}
				}
				else
				{
					Error = token.Line + "Command or label expected";
					return ErrorTuple;
				}
			}
			else if (State == WaitingFirstOpOrSize || State == WaitingFirstOp || State == WaitingFirstOpOrEnd)
			{
				return CheckFirstOp( token );
			}
			else if (State == WaitingCommaOrEnd || State == WaitingComma)
			{
				if (token.Value == ",")
				{
					State = WaitingSecondOp;
					ActualCommand = _overrides.twoOp;
					return WaitingNextToken;
				}
				else
				{
					if (State == WaitingComma)
					{
						Error = token.Line + ", expected";
						return ErrorTuple;
					}
					State = Finished;
					ActualCommand = _overrides.oneOp;
					if (Op1.IsRegister)
					{
						OperandsMode = Emulator.OperandsMode.RegReg;
					}
					return FinishedAndDontAdd;
				}
			}
			else if (State == WaitingSecondOp)
			{
				return CheckSecondOp( token );
			}
			else
			{
				Error = token.Line + "End expected";
				return ErrorTuple;
			}
		}

		(bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode) CheckSecondOp( Token token )
		{
			State = Finished;
			if (token.Type == TokenType.Register)
			{
				Op2 = new Operand { IsRegister = true, RegisterIndex = CPU.GetRegisterIndex( token.Value ) };
				if (SizeMode == null)
				{
					SizeMode = CPU.GetRegisterSize( token.Value );
				}
				else if (SizeMode != CPU.GetRegisterSize( token.Value ))
				{
					Error = token.Line + "Different sizes";
					return ErrorTuple;
				}
				State = Finished;
				if(Op1.IsAddress)
				{
					OperandsMode = Emulator.OperandsMode.MemReg;
				}
				else
				{
					OperandsMode = Emulator.OperandsMode.RegReg;
				}
				return FinishedAndAdd;
			}
			else if (token.Value == "[")
			{
				if (Op1.IsAddress)
				{
					Error = token.Line + "2 address operands";
					return ErrorTuple;
				}
				Op2 = new Operand { IsAddress = true, AddressNode = new AddressNode() };
				OperandsMode = Emulator.OperandsMode.RegMem;
				State = Finished;
				return ChildAndEnd( Op2.AddressNode );
			}
			else
			{
				if (Op1.IsAddress)
				{
					Error = token.Line + "Address and immediate operands";
					return ErrorTuple;
				}
				Op2 = new Operand { IsImmediate = true, Immediate = new SimpleValueNode() };
				OperandsMode = Emulator.OperandsMode.RegImm;
				State = Finished;
				return ChildAndEnd( Op2.Immediate );
			}
		}

		(bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode) CheckFirstOp( Token token )
		{
			if (token.Type == TokenType.Register)
			{
				Op1 = new Operand { IsRegister = true, RegisterIndex = CPU.GetRegisterIndex( token.Value ) };
				SizeMode = CPU.GetRegisterSize( token.Value );
				if (_overrides.twoOp == null)
				{
					State = Finished;
					ActualCommand = _overrides.oneOp;
					OperandsMode = Emulator.OperandsMode.RegReg;
					return FinishedAndAdd;
				}
				else if (_overrides.oneOp == null)
				{
					State = WaitingComma;
					return WaitingNextToken;
				}
				else
				{
					State = WaitingCommaOrEnd;
					return OptionalContinuation;
				}
			}
			else if (token.Value == "[")
			{
				Op1 = new Operand { IsAddress = true, AddressNode = new AddressNode() };
				OperandsMode = Emulator.OperandsMode.MemReg;
				if (_overrides.twoOp == null)
				{
					State = Finished;
					ActualCommand = _overrides.oneOp;
					return ChildAndEnd( Op1.AddressNode );
				}
				else
				{
					State = WaitingCommaOrEnd;
					return ChildWithOptional( Op1.AddressNode );
				}
			}
			else if (State == WaitingFirstOpOrSize && (token.Value == "int" || token.Value == "short" || token.Value == "byte"))
			{
				if (token.Value == "int")
				{
					SizeMode = Emulator.SizeMode.FourBytes;
				}
				else if (token.Value == "short")
				{
					SizeMode = Emulator.SizeMode.TwoBytes;
				}
				else
				{
					SizeMode = Emulator.SizeMode.OneByte;
				}
				State = WaitingFirstOp;
				return WaitingNextToken;
			}
			else
			{
				if (State == WaitingFirstOpOrEnd && Command.CommandCodeExists( token.Value ))
				{
					State = Finished;
					ActualCommand = _overrides.noOp;
					return FinishedAndDontAdd;
				}
				Op1 = new Operand { IsImmediate = true, Immediate = new SimpleValueNode() };
				if (_overrides.oneOp == null)
				{
					Error = token.Line + "Immediate can't be first operand in 2-op command";
					return ErrorTuple;
				}
				ActualCommand = _overrides.oneOp;
				OperandsMode = Emulator.OperandsMode.RegImm;
				State = Finished;
				return ChildAndEnd( Op1.Immediate );
			}
		}

		(bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode) UpdateAfterCommandCode()
		{
			if (_overrides.noOp == null && _overrides.oneOp == null)
			{
				ActualCommand = _overrides.twoOp;
			}
			if (_overrides.noOp == null)
			{
				State = WaitingFirstOpOrSize;
				return WaitingNextToken;
			}
			else if (_overrides.oneOp != null || _overrides.twoOp != null)
			{
				State = WaitingFirstOpOrEnd;
				return OptionalContinuation;
			}
			else
			{
				State = Finished;
				ActualCommand = _overrides.noOp;
				return FinishedAndAdd;
			}
		}

		protected override ParsingError? OnParsingEnded()
		{
			if (SizeMode == null)
			{
				SizeMode = Emulator.SizeMode.FourBytes;
			}
			return null;
		}
	}
}
