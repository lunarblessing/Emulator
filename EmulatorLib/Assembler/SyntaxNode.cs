using System;
using System.Collections.Generic;

namespace Emulator.Assembler
{
	public abstract class SyntaxNode : SyntaxElement
	{

		public ParsingError? Error { get; protected set; }
		public List<SyntaxElement> Children { get; set; } = new List<SyntaxElement>();
		protected int State { get; set; }

		//return index of first token, not belonging to this node
		public int ParseTokens( Token[] tokens, int startIndex, out bool successful )
		{
			if (tokens.Length == 0 || startIndex >= tokens.Length)
			{
				Error = "Unexpected end of file" ;
				successful = false;
				return startIndex;
			}
			int i = startIndex;
			while (i < tokens.Length)
			{
				var (forcedCont, optionalCont, error, add, newNode) = ParseToken( tokens[i] );
				if (add)
				{
					Children.Add( tokens[i] );
				}
				if (error)
				{
					successful = false;
					return i;
				}
				if (newNode != null)
				{
					i = newNode.ParseTokens( tokens, i, out successful );
					Children.Add( newNode );
					if (!successful)
					{
						Error = newNode.Error;
						return i;
					}
				}
				else
				{
					i++;
				}
				bool doNextToken = forcedCont || (optionalCont && i < tokens.Length);
				if (!doNextToken)
				{
					if (Error != null)
					{
						successful = false;
						return i;
					}
					OnParsingEnded();
					successful = true;
					if(!add && newNode == null)
					{
						return i - 1;
					}
					return i;
				}
				else
				{
					continue;
				}
			}
			// only way to be here is if doNextToken was true, but tokens array ended
			Error = "Unexpected end of file";
			successful = false;
			return tokens.Length;
		}

		// add - add this token to current node
		// error - error happened
		// newNode - this token started a new child node
		// optionalCont - next token can optionally belong to current node
		// forcedCont - next token is obliged to belong to current node
		protected abstract (bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode) ParseToken( Token token );

		protected virtual ParsingError? OnParsingEnded()
		{
			return null;
		}

		protected static (bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode)
			ChildWithOptional( SyntaxNode child )
		{
			return (false, true, false, false, child);
		}

		protected static (bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode)
			ChildWithObligatory( SyntaxNode child )
		{
			return (true, false, false, false, child);
		}

		protected static (bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode)
			ChildAndEnd( SyntaxNode child )
		{
			return (false, false, false, false, child);
		}

		protected static (bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode)
			ErrorTuple = (false, false, true, true, null);
		protected static (bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode)
			WaitingNextToken = (true, false, false, true, null);
		protected static (bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode)
			OptionalContinuation = (false, true, false, true, null);
		protected static (bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode)
			FinishedAndAdd = (false, false, false, true, null);
		protected static (bool forcedCont, bool optionalCont, bool error, bool add, SyntaxNode? newNode)
			FinishedAndDontAdd = (false, false, false, false, null);

		protected static bool LiteralValueTooBig( string value, uint size )
		{
			try
			{
				long intValue = long.Parse( value );
				return LiteralValueTooBig( intValue, size );
			}
			catch (OverflowException)
			{
				return true;
			}
		}

		protected static bool LiteralValueTooBig( long value, uint size )
		{
			return (size == 2 && (value > ushort.MaxValue || value < short.MinValue))
				|| (size == 1 && (value > byte.MaxValue || value < sbyte.MinValue))
				|| (size == 4 && (value > uint.MaxValue || value < int.MinValue));
		}
	}
}
