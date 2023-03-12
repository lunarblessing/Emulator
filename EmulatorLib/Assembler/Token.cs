using System;
using System.Linq;

namespace Emulator.Assembler
{
	public class Token : SyntaxElement
	{
		static string[] operators = { "=", "+", "-", "*", "[", "]" };
		static string[] separators = { "," };

		public int StartIndex { get; set; }
		public int LastIndex { get; set; }
		public int Line { get; set; }
		public string Value { get; set; }
		public TokenType Type { get; set; }

		public Token()
		{

		}

		public int ParseToken( string program, int startIndex, int lineIndex, out bool successful )
		{
			void _SetLastIndexAndValue( int lastIndex )
			{
				LastIndex = lastIndex;
				Value = program.Substring( StartIndex, LastIndex - StartIndex ).ToLower().Trim();
			}

			successful = true;
			while (startIndex < program.Length && char.IsWhiteSpace( program[startIndex] ))
			{
				if (program[startIndex] == '\n')
				{
					lineIndex++;
				}
				startIndex++;
			}
			StartIndex = startIndex;
			Line = lineIndex;
			if (startIndex >= program.Length)
			{
				successful = false;
				return StartIndex;
			}
			char firstChar = program[StartIndex];
			int index = StartIndex + 1;
			if (char.IsDigit( firstChar ))
			{
				Type = TokenType.Literal;
				while (index < program.Length && char.IsDigit( program[index] ))
				{
					index++;
				}
				_SetLastIndexAndValue( index );
			}
			else if (firstChar == '/')
			{
				if (index < program.Length && program[index] == '/')
				{
					Type = TokenType.Comment;
					while (index < program.Length && program[index] != '\n')
					{
						index++;
					}
					_SetLastIndexAndValue( index );
				}
			}
			else if (operators.Contains( firstChar.ToString() ))
			{
				Type = TokenType.Operator;
				_SetLastIndexAndValue( index );
			}
			else if (separators.Contains( firstChar.ToString() ))
			{
				Type = TokenType.Separator;
				_SetLastIndexAndValue( index );
			}
			else if (char.IsLetter( firstChar ) || firstChar == '_' || firstChar == '.')
			{
				while (index < program.Length && (char.IsLetterOrDigit( program[index] )
					|| program[index] == '_' || program[index] == '.' || program[index] == ':'))
				{
					index++;
					if (program[index - 1] == ':')
					{
						break;
					}
				}
				_SetLastIndexAndValue( index );
				if (CPU.IsValidRegisterName( Value ))
				{
					Type = TokenType.Register;
				}
				else if (Lexer.Keywords.Contains( Value.ToLower() ))
				{
					Type = TokenType.Keyword;
				}
				else if (Value[^1] == ':')
				{
					Type = TokenType.Label;
				}
				else
				{
					Type = TokenType.Identifier;
				}
			}
			else
			{
				_SetLastIndexAndValue( index );
				Type = TokenType.Unknown;
				successful = false;
			}
			return LastIndex;
		}

		public override string ToString()
		{
			return $"{{{Type}}}: {Value}";
		}
	}
}
