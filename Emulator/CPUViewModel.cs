using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emulator.Assembler.Compiler;
using System.IO;
using Emulator;

namespace EmulatorUI
{
	public class CPUViewModel
	{
		const int StackDepth = 32;
		CPU CPU { get; set; }
		CPUControl Control { get; set; }

		public CPUViewModel( byte[] memory, uint entryPoint, uint stackPointer, uint minStack )
		{
			CPU = new CPU( memory, entryPoint, stackPointer );
			FileStream file = new FileStream( "out.txt", FileMode.Create );
			CPU.OutputStream = file;
		}

		public CPUViewModel( Compiler comp )
		{
			CPU = comp.StartCPU();
			FileStream file = new FileStream( "out.txt", FileMode.Create );
			CPU.OutputStream = file;
		}

		public void SetControl( CPUControl control )
		{
			Control = control;
			Control.ViewModel = this;
			if (CPU != null)
			{
				StartProcessing();
			}
		}

		public void Output( string str )
		{
			if (Control == null)
			{
				return;
			}
			Control.Dispatcher.Invoke( () => Control.WriteOutputLine( str ) );
		}

		public void RunProgram()
		{
			if (CPU == null)
			{
				return;
			}
			Task.Factory.StartNew( () =>
			{
				while (CPU.DoNextCommand())
				{

				}
				UpdateUI();
			} );
		}

		public void DoNextCommand()
		{
			if (CPU == null)
			{
				return;
			}
			CPU.DoNextCommand();
			UpdateUI();
		}

		void StartProcessing()
		{
			for (int i = 0; i < CPU.RegistersCount; i++)
			{
				Control.AddNewRegister( CPU.GetRegisterName( (uint)i, SizeMode.FourBytes ).ToUpper(), (int)CPU.GeneralRegisters[i], 0 );
				Control.AddNewRegister( CPU.GetRegisterName( (uint)i, SizeMode.TwoBytes ).ToUpper(), (short)CPU.GeneralRegisters[i], 1 );
				Control.AddNewRegister( CPU.GetRegisterName( (uint)i, SizeMode.OneByte ).ToUpper(), (sbyte)CPU.GeneralRegisters[i], 2 );
			}
			Control.AddNewRegister( "IP", (int)CPU.InstructionPointer, 0 );
			Control.AddNewRegister( "ZF", Convert.ToInt32( CPU.ZF ), 0 );
			Control.AddNewRegister( "OF", Convert.ToInt32( CPU.OF ), 0 );
			Control.AddNewRegister( "SF", Convert.ToInt32( CPU.SF ), 0 );
			Control.AddNewRegister( "CF", Convert.ToInt32( CPU.CF ), 0 );
			Control.InitializeStackElements( StackDepth );
			int commandOffset = 0;
			int emptyCommandsInARow = 0;
			List<CommandAsmUI> commands = new List<CommandAsmUI>( 500 );
			for (int i = 0; i < 1000; i++)
			{
				string asm = CPU.GetAsmForCommandAt( (uint)(CPU.InstructionPointer + commandOffset), out int length );
				if (CPU.IsCommandTheEndCommand( (uint)(CPU.InstructionPointer + commandOffset) ))
				{
					emptyCommandsInARow++;
					asm = "end";
				}
				else
				{
					emptyCommandsInARow = 0;
				}
				if (emptyCommandsInARow >= 50)
				{
					break;
				}
				commands.Add( new CommandAsmUI( (CPU.InstructionPointer + commandOffset).ToString(), asm ) );
				commandOffset += length;
			}
			Control.SetProgram( commands.ToArray() );
			UpdateUI();
		}

		void UpdateUI()
		{
			int[] stack = new int[StackDepth];
			for (int i = 0; i < StackDepth; i++)
			{
				stack[i] = CPU.GetStackValue( i * 4 );
			}
			var fields = CPU.GetFieldsCommand( CPU.InstructionPointer );
			Control.Dispatcher.Invoke( () =>
			{
				Control.UnhighlightRegisters();
				for (int i = 0; i < CPU.RegistersCount; i++)
				{
					Control.UpdateRegisterValue( CPU.GetRegisterName( (uint)i, SizeMode.FourBytes ).ToUpper(), (int)CPU.GeneralRegisters[i] );
					Control.UpdateRegisterValue( CPU.GetRegisterName( (uint)i, SizeMode.TwoBytes ).ToUpper(), (short)CPU.GeneralRegisters[i] );
					Control.UpdateRegisterValue( CPU.GetRegisterName( (uint)i, SizeMode.OneByte ).ToUpper(), (sbyte)CPU.GeneralRegisters[i] );
				}
				Control.HighlightCommandAtAddress( CPU.InstructionPointer.ToString() );
				Control.UpdateStackValues( stack );
				Control.UpdateRegisterValue( "IP", (int)CPU.InstructionPointer );
				Control.UpdateRegisterValue( "CF", Convert.ToInt32( CPU.CF ) );
				Control.UpdateRegisterValue( "OF", Convert.ToInt32( CPU.OF ) );
				Control.UpdateRegisterValue( "ZF", Convert.ToInt32( CPU.ZF ) );
				Control.UpdateRegisterValue( "SF", Convert.ToInt32( CPU.SF ) );
				Control.SetCommandFields( fields );
			} );
		}
	}

	public struct CommandAsmUI
	{
		public string address;
		public string asm;

		public CommandAsmUI( string address, string asm )
		{
			this.address = address;
			this.asm = asm;
		}
	}
}
