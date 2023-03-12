namespace Emulator.Assembler
{

	public static class Extensions
	{
		public static string ToString(this TokenType tokenType)
		{
			switch (tokenType)
			{
				case TokenType.Unknown:
					return "Unknown";
				case TokenType.Keyword:
					return "Keyword";
				case TokenType.Register:
					return "Register";
				case TokenType.Identifier:
					return "Identifier";
				case TokenType.Literal:
					return "Literal";
				case TokenType.Separator:
					return "Separator";
				case TokenType.Operator:
					return "Operator";
				case TokenType.Label:
					return "Label";
				default:
					return "Unknown";
			}
		}
	}

	public enum LabelType
	{
		CodeLabel, Var
	}

	public enum TokenType
	{
		Unknown, Keyword, Register, Identifier, Literal, Separator, Operator, Label, Comment
	}
}
