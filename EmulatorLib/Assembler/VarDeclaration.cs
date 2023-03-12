using System.Linq;

namespace Emulator.Assembler
{
	public class VarDeclaration : SyntaxNode
	{
		const int StartState = 0;
		const int ArrayClassifierOrIdExpected = 1;
		const int ArraySizeExpected = 2;
		const int ClosedBracketExpected = 3;
		const int IdExpected = 4;
		const int EqualsOrEndExpected = 5;
		const int SingleValueExpected = 6;
		const int ArrayOfValuesExpected = 7;
		const int Finished = 8;

		public string Name { get; set; }
		public bool IsArray { get; set; }
		public uint VarSize { get; set; }
		public SimpleValueNode? ArrayLength { get; set; }
		public SimpleValueNode SingleValue { get; set; }
		public SimpleValueNode[] ArrayOfValues { get; set; }

		bool hasValue;

		public VarDeclaration()
		{
			State = StartState;
		}


		protected override (bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode) ParseToken( Token token )
		{
			if (State == StartState)
			{
				if (token.Value == "int")
				{
					VarSize = 4;
				}
				else if (token.Value == "short")
				{
					VarSize = 2;
				}
				else if (token.Value == "byte")
				{
					VarSize = 1;
				}
				else
				{
					Error = token.Line + "Expected type";
					return ErrorTuple;
				}
				State = ArrayClassifierOrIdExpected;
				return WaitingNextToken;
			}
			else if (State == ArrayClassifierOrIdExpected)
			{
				if (token.Value == "[")
				{
					State = ArraySizeExpected;
					IsArray = true;
					return WaitingNextToken;
				}
				else if (token.Type == TokenType.Identifier)
				{
					Name = token.Value;
					State = EqualsOrEndExpected;
					return OptionalContinuation;
				}
				else
				{
					Error = token.Line + "Expected id or [";
					return ErrorTuple;
				}
			}
			else if (State == ArraySizeExpected)
			{
				ArrayLength = new SimpleValueNode();
				State = ClosedBracketExpected;
				return ChildWithObligatory( ArrayLength );
			}
			else if (State == ClosedBracketExpected)
			{
				if (token.Value == "]")
				{
					State = IdExpected;
					return WaitingNextToken;
				}
				else
				{
					Error = token.Line + "] expected";
					return ErrorTuple;
				}
			}
			else if (State == IdExpected)
			{
				if (token.Type == TokenType.Identifier)
				{
					Name = token.Value;
					State = EqualsOrEndExpected;
					return OptionalContinuation;
				}
				else
				{
					Error = token.Line + "id expected";
					return ErrorTuple;
				}
			}
			else if (State == EqualsOrEndExpected)
			{
				if (token.Value == "=")
				{
					State = IsArray ? ArrayOfValuesExpected : SingleValueExpected;
					hasValue = true;
					return WaitingNextToken;
				}
				else
				{
					State = Finished;
					return FinishedAndDontAdd;
				}
			}
			else if (State == SingleValueExpected)
			{
				State = Finished;
				return ChildAndEnd( new SimpleValueNode() );
			}
			else if (State == ArrayOfValuesExpected)
			{
				State = Finished;
				if (ArrayLength.IsLiteral)
				{
					if (LiteralValueTooBig( ArrayLength.Literal, 2 ) || ArrayLength.Literal < 1)
					{
						Error = token.Line + "Wrong array size";
						return ErrorTuple;
					}
					return ChildAndEnd( new ArrayOfValuesNode( (int)ArrayLength.Literal ) );
				}
				return ChildAndEnd( new ArrayOfValuesNode( ushort.MaxValue ) );
			}
			else
			{
				Error = token.Line + "End of var declaration expected";
				return ErrorTuple;
			}
		}

		protected override ParsingError? OnParsingEnded()
		{
			if (!hasValue)
			{
				return null;
			}
			if (!IsArray)
			{
				if (Children.Last() is SimpleValueNode value)
				{
					SingleValue = value;
					if (value.IsLiteral && LiteralValueTooBig( value.Literal, VarSize ))
					{
						Error = "Too big number";
						return Error;
					}
					else
					{
						return null;
					}
				}
				else
				{
					Error = " Value is not detected";
					return Error;
				}
			}
			else
			{
				var lastChild = Children.Last();
				if (lastChild is ArrayOfValuesNode array)
				{
					ArrayOfValues = array.Values;
					foreach (var item in ArrayOfValues)
					{
						if (item.IsLiteral && LiteralValueTooBig( item.Literal, VarSize ))
						{
							Error = "Too big number";
							return Error;
						}
					}
					return null;
				}
				else
				{
					Error = "Array of values is not detected";
					return Error;
				}
			}
		}
	}
}
