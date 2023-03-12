namespace Emulator.Assembler
{
	public abstract class SyntaxElement
	{
		public SyntaxNode? AsNode()
		{
			return this as SyntaxNode;
		}

		public Token? AsToken()
		{
			return this as Token;
		}
	}
}
