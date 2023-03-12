using System.Linq;

namespace Emulator.Assembler
{
	public class ArrayOfValuesNode : SyntaxNode
	{
		const int ValueExpected = 1;
		const int CommaOrEndExpected = 2;
		const int EndExpected = 3;

		int MaxValues { get; set; }
		int NextIndex { get; set; }

		public SimpleValueNode[] Values { get; set; }

		public ArrayOfValuesNode( int maxValuesCount )
		{
			MaxValues = maxValuesCount;
			State = ValueExpected;
		}

		protected override (bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode) ParseToken( Token token )
		{
			if (State == ValueExpected)
			{
				if(NextIndex + 1 >= MaxValues)
				{
					State = EndExpected;
				}
				else
				{
					State = CommaOrEndExpected;
				}
				return ChildWithOptional( new SimpleValueNode() );
			}
			else if (State == CommaOrEndExpected)
			{
				if (token.Value == ",")
				{
					State = ValueExpected;
					return WaitingNextToken;
				}
				else
				{
					return FinishedAndDontAdd;
				}
			}
			else
			{
				if (token.Value == ",")
				{
					Error = token.Line + "Too many elements";
					return ErrorTuple;
				}
				return FinishedAndDontAdd;
			}
		}

		protected override ParsingError? OnParsingEnded()
		{
			Values = Children.OfType<SimpleValueNode>().ToArray();
			return null;
		}
	}
}
