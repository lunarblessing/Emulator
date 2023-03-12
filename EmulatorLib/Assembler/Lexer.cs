using System.Collections.Generic;

namespace Emulator.Assembler
{
	public class Lexer
	{
		internal static string[] Keywords = { "const", "int", "short", "byte", "section" };

		public ParsingError? Error { get; private set; }

		public Token[] Parse( string program, out bool successful )
		{
			successful = true;
			List<Token> list = new List<Token>( 1000 );
			int index = 0;
			int lineIndex = 1;
			while (index < program.Length)
			{
				Token token = new Token();
				index = token.ParseToken( program, index, lineIndex, out bool tokenSuccess );
				lineIndex = token.Line;
				if (tokenSuccess)
				{
					if (token.Type != TokenType.Comment)
					{
						list.Add( token );
					}
				}
				else if (token.StartIndex >= program.Length)
				{
					break;
				}
				else
				{
					successful = false;
					Error = $"Error parsing token in line {token.Line}";
					break;
				}
			}
			return list.ToArray();
		}
	}
}
