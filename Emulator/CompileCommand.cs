using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Emulator.Assembler;
using Emulator.Assembler.Compiler;

namespace EmulatorUI
{
	public class CompileCommand : ICommand
	{
		public event EventHandler? CanExecuteChanged;

		public bool CanExecute( object? parameter )
		{
			return true;
		}

		public void Execute( object? parameter )
		{
			if (parameter is CompileCommandParams parameters)
			{
				var control = parameters.control;
				var program = parameters.programText;
				if (control == null || program == null)
				{
					return;
				}
				Compile( control, program );
			}
		}

		void Compile( AssemblerControl control, string program )
		{
			Task.Factory.StartNew( () =>
			{
				Lexer l = new Lexer();
				var tokens = l.Parse( program, out bool tokensSuccessful );
				if (tokensSuccessful)
				{
					RootNode root = new RootNode();
					root.ParseTokens( tokens, 0, out bool successful );
					if (successful)
					{
						Compiler comp = new Compiler();
						if (comp.ProcessRoot( root ))
						{
							CPUViewModel vm = new CPUViewModel( comp );
							control.StartCPU( vm );
						}
						else
						{
							control.WriteCompilationError( comp.Error.Desc );
						}
					}
					else
					{
						control.WriteCompilationError( root.Error.Desc );
					}
				}
				else
				{
					control.WriteCompilationError( l.Error.Desc );
				}
			} );
		}
	}
}
