namespace Emulator.Assembler
{
	public class RootNode : SyntaxNode
	{
		const int StartState = 0;
		const int SectionsExpected = 1;

		public RootNode()
		{
			State = StartState;
		}

		protected override (bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode) ParseToken( Token token )
		{
			if (token.Value == "const")
			{
				if (State == StartState)
				{
					ConstDeclaration decl = new ConstDeclaration();
					return ChildWithOptional( decl );
				}
				else
				{
					Error = token.Line + "section was expected";
					return ErrorTuple;
				}
			}
			else if (token.Value == "section")
			{
				State = SectionsExpected;
				SectionNode section = new SectionNode();
				return ChildWithOptional( section );
			}
			else
			{
				Error = token.Line + "Expected const or section declaration" ;
				return ErrorTuple;
			}
		}
	}
}
