using System.Linq;

namespace Emulator.Assembler
{
	public class SectionNode : SyntaxNode
	{

		const int StartState = 0;
		const int ExpectingId = 1;
		const int ExpectingVarsOrCode = 2;
		const int ExpectingVar = 3;
		const int ExpectingCode = 4;
		const int Finished = 5;

		public string Name { get; set; }
		public bool IsVarSection { get; set; }
		public bool IsCodeSection { get; set; }
		public VarDeclaration[]? Vars { get; set; }
		public CommandNode[]? Commands { get; set; }

		public SectionNode()
		{
			State = StartState;
		}

		protected override (bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode) ParseToken( Token token )
		{
			if (State == StartState)
			{
				if (token.Value == "section")
				{
					State = ExpectingId;
					return WaitingNextToken;
				}
				else
				{
					Error = token.Line +  "Expected 'section' keyword";
					return ErrorTuple;
				}
			}
			else if (State == ExpectingId)
			{
				if (token.Type == TokenType.Identifier)
				{
					Name = token.Value;
					State = ExpectingVarsOrCode;
					return WaitingNextToken;
				}
				else
				{
					Error = token.Line + "Expected identifier";
					return ErrorTuple;
				}
			}
			else if (State == ExpectingVarsOrCode)
			{
				if (token.Type == TokenType.Keyword)
				{
					State = ExpectingVar;
					IsVarSection = true;
					return ChildWithOptional( new VarDeclaration() );
				}
				else if (token.Type == TokenType.Identifier)
				{
					State = ExpectingCode;
					IsCodeSection = true;
					return ChildWithOptional( new CommandNode() );
				}
				else
				{
					Error = token.Line + "Expected var declaration or code";
					return ErrorTuple;
				}
			}
			else if (State == ExpectingVar)
			{
				if(token.Value == "section")
				{
					State = Finished;
					return FinishedAndDontAdd;
				}
				else if (token.Type == TokenType.Keyword)
				{
					return ChildWithOptional( new VarDeclaration() );
				}
				else
				{
					Error = token.Line + "Expected var declaration";
					return ErrorTuple;
				}
			}
			else if(State == ExpectingCode)
			{
				if (token.Value == "section")
				{
					State = Finished;
					return FinishedAndDontAdd;
				}
				if (token.Type == TokenType.Identifier || token.Type == TokenType.Label)
				{
					return ChildWithOptional( new CommandNode() );
				}
				else
				{
					Error = token.Line + "Expected code";
					return ErrorTuple;
				}
			}
			else
			{
				Error = token.Line + "Section finished";
				return ErrorTuple;
			}
		}

		protected override ParsingError? OnParsingEnded()
		{
			if(IsVarSection)
			{
				Vars = Children.OfType<VarDeclaration>().ToArray();
			}
			else if(IsCodeSection)
			{
				Commands = Children.OfType<CommandNode>().ToArray();
			}
			else
			{
				Error = "Section doesn't know whether it's code or data";
				return Error;
			}
			return null;
		}
	}
}
