namespace Emulator.Assembler
{
	public class SimpleValueNode : SyntaxNode
	{
		const int StartState = 0;
		const int LiteralOrIdExpected = 1;
		const int BracketOrEndExpected = 2;
		const int IndexValueExpected = 3;
		const int ClosedBracketExpected = 4;
		const int Finished = 5;

		public bool IsLiteral { get; set; }
		public bool IsIdentifier { get; set; }
		public long Literal { get; set; }
		public string Identifier { get; set; }
		public bool PositiveIdentifier { get; set; }
		public SimpleValueNode? IdentifierArrayIndex { get; set; }

		protected override (bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode) ParseToken( Token token )
		{
			if (State == StartState)
			{
				if (token.Type == TokenType.Identifier)
				{
					IsIdentifier = true;
					Identifier = token.Value;
					PositiveIdentifier = true;
					State = BracketOrEndExpected;
					return OptionalContinuation;
				}
				else if (token.Type == TokenType.Literal)
				{
					IsLiteral = true;
					Literal = long.Parse( token.Value );
					State = Finished;
					return FinishedAndAdd;
				}
				else if (token.Value == "-")
				{
					PositiveIdentifier = false;
					State = LiteralOrIdExpected;
					return WaitingNextToken;
				}
				else
				{
					Error = token.Line + "Expected value";
					return ErrorTuple;
				}
			}
			else if (State == LiteralOrIdExpected)
			{
				if (token.Type == TokenType.Identifier)
				{
					IsIdentifier = true;
					Identifier = token.Value;
					State = BracketOrEndExpected;
					return OptionalContinuation;
				}
				else if (token.Type == TokenType.Literal)
				{
					IsLiteral = true;
					Literal = -long.Parse( token.Value );
					State = Finished;
					return FinishedAndAdd;
				}
				else
				{
					Error = token.Line + "Expected value";
					return ErrorTuple;
				}
			}
			else if (State == BracketOrEndExpected)
			{
				if (token.Value == "[")
				{
					State = IndexValueExpected;
					return WaitingNextToken;
				}
				else
				{
					State = Finished;
					return FinishedAndDontAdd;
				}
			}
			else if (State == IndexValueExpected)
			{
				IdentifierArrayIndex = new SimpleValueNode();
				State = ClosedBracketExpected;
				return ChildWithObligatory( IdentifierArrayIndex );
			}
			else if (State == ClosedBracketExpected)
			{
				if (token.Value == "]")
				{
					State = Finished;
					return FinishedAndAdd;
				}
				else
				{
					Error = token.Line + "Expected ]";
					return ErrorTuple;
				}
			}
			else
			{
				Error = token.Line + "Value already finished";
				return ErrorTuple;
			}
		}
	}
}
