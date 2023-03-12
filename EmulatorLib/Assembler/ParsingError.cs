using System.Linq;

namespace Emulator.Assembler
{
	public class ParsingError
	{
		public string? Desc { get; set; }
		public int Line { get; set; }

		public ParsingError( string? desc, int line )
		{
			Desc = desc;
			Line = line;
		}

		public static implicit operator ParsingError( string value )
		{
			if (value != null && value.Length > 0 && char.IsDigit( value[0] ))
			{
				int line = int.Parse( new string( value.TakeWhile( char.IsDigit ).ToArray() ) );
				return new ParsingError( value, line );
			}
			return new ParsingError( value, 0 );
		}
	}
}
