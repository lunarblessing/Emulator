namespace Emulator.Assembler
{
	public class ConstDeclaration : SyntaxNode
	{

		const int StartState = 0;
		const int ExpectingId = 1;
		const int ExpectingEquals = 2;
		const int ExpectingValue = 3;
		const int ExpectingLiteralForNegative = 4;
		const int Finished = 5;

		public string Name { get; protected set; }
		public long Value { get; protected set; }

		public ConstDeclaration()
		{
			State = StartState;
		}

		protected override (bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode) ParseToken( Token token )
		{
			if (State == StartState)
			{
				if (token.Value == "const")
				{
					State = ExpectingId;
					return WaitingNextToken;
				}
				else
				{
					Error = token.Line + "Expected 'const' keyword";
					return ErrorTuple;
				}
			}
			else if (State == ExpectingId)
			{
				if (token.Type == TokenType.Identifier)
				{
					State = ExpectingEquals;
					Name = token.Value;
					return WaitingNextToken;
				}
				else
				{
					Error = token.Line + "Expected identifier";
					return ErrorTuple;
				}
			}
			else if (State == ExpectingEquals)
			{
				if (token.Value == "=")
				{
					State = ExpectingValue;
					return WaitingNextToken;
				}
				else
				{
					Error = token.Line + "Expected '='";
					return ErrorTuple;
				}
			}
			else if (State == ExpectingValue)
			{
				if (token.Type == TokenType.Literal)
				{
					State = Finished;
					Value = long.Parse( token.Value );
					return FinishedAndAdd;
				}
				else if(token.Value == "-")
				{
					State = ExpectingLiteralForNegative;
					return WaitingNextToken;
				}
				else
				{
					Error = token.Line + "Expected literal";
					return ErrorTuple;
				}
			}
			else if(State == ExpectingLiteralForNegative)
			{
				if (token.Type == TokenType.Literal)
				{
					State = Finished;
					Value = -long.Parse( token.Value );
					return FinishedAndAdd;
				}
				else
				{
					Error = token.Line + "Expected literal";
					return ErrorTuple;
				}
			}
			else
			{
				Error = token.Line + "Expected declaration to be finished";
				return ErrorTuple;
			}
		}
	}
}
