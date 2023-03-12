using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;


namespace Emulator
{
	/// <summary>
	/// Main emulator class containing program memory, registers, and capable of parsing and executing byte code.
	/// </summary>
	public class CPU
	{

		/// <summary>
		/// Count of general purpose registers, length of <see cref="GeneralRegisters"/>.
		/// </summary>
		public const int RegistersCount = 8;

		/// <summary>
		/// Code of instruction which is allowed to work with 4-bytes constants. Used by compiler and in CPU class.
		/// </summary>
		internal const string MovInstructionCode = "mov";

		/// <summary>
		/// Code of end instruction, which stops the program from executing, so it doesn't run infinite loop.
		/// </summary>
		internal const string EndInstructionCode = "end";

		/// <summary>
		/// Smallest address at which program code or memory is written by compiler.
		/// </summary>
		internal const uint SmallestAddress = 1 << 12;

		/// <summary>
		/// Count of bytes which is reserved for stack.
		/// </summary>
		internal const uint StackReserve = 1 << 15;


		////////// Static


		/// <summary>
		/// Names of all possible registers, not guaranteed to be in any particular order. <br></br> 
		/// </summary>
		static readonly string[] allPossibleRegisters = {
			"eax", "ax", "ah", "al", "ebx", "bx", "bh", "bl",
			"ecx", "cx", "ch", "cl", "edx", "dx", "dh", "dl",
			"esi", "si", "sih", "sil", "edi", "di", "dih", "dil",
			"ebp", "bp", "bph", "bpl", "esp", "sp", "sph", "spl" };

		/// <summary>
		/// Contains names of registers (register index is key). <br></br>
		/// Order of each value array should be in order of <see cref="SizeMode"/> enum.
		/// </summary>
		// Preserve order in each array according to SizeMode enum
		static readonly Dictionary<uint, string[]> registersByIndex = new() {
			{(uint)Registers.A, new string[] { "eax", "ax", "ah", "al" } },
			{(uint)Registers.B, new string[] { "ebx", "bx", "bh", "bl" } },
			{(uint)Registers.C, new string[] { "ecx", "cx", "ch", "cl"} },
			{(uint)Registers.D, new string[] { "edx", "dx", "dh", "dl"} },
			{(uint)Registers.SI, new string[] { "esi", "si", "sih", "sil"} },
			{(uint)Registers.DI, new string[] { "edi", "di", "dih", "dil"} },
			{(uint)Registers.BP, new string[] { "ebp", "bp", "bph", "bpl"} },
			{(uint)Registers.SP, new string[] { "esp", "sp", "sph", "spl"} },
		};


		/// <summary>
		/// Checks whether specified name is valid register name, case-insensitive.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		internal static bool IsValidRegisterName( string name )
		{
			return allPossibleRegisters.Contains( name.ToLower() );
		}


		/// <summary>
		/// Checks whether register with specified name is 4-byte register, case-insensitive.
		/// </summary>
		/// <param name="register"></param>
		/// <returns></returns>
		internal static bool Is32bitRegister( string register )
		{
			return register.ToLower().StartsWith( 'e' ) && allPossibleRegisters.Contains( register.ToLower() );
		}


		/// <summary>
		/// Returns index of register with specified name, case-insensitive;
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		internal static uint GetRegisterIndex( string name )
		{
			string lowerName = name.ToLower();
			return registersByIndex.First( kv => kv.Value.Contains( lowerName ) ).Key;
		}


		/// <summary>
		/// Returns size of register with specified name, case-insensitive.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		internal static SizeMode GetRegisterSize( string name )
		{
			string lowerName = name.ToLower();
			return (SizeMode)Array.IndexOf( registersByIndex.First( kv => kv.Value.Contains( lowerName ) ).Value, name );
		}


		////////// Not static


		/// <summary>
		/// Values of general registers.
		/// </summary>
		public uint[] GeneralRegisters { get; private set; }

		/// <summary>
		/// Address of currently executing instruction or next instruction to be executed.
		/// </summary>
		public uint InstructionPointer { get; private set; }

		/// <summary>
		/// Memory of program.
		/// </summary>
		public byte[] Memory { get; private set; }

		/// <summary>
		/// Output stream of program.
		/// </summary>
		public Stream? OutputStream { get; set; }

		/// <summary>
		/// Carry flag.
		/// </summary>
		public bool CF { get; set; }

		/// <summary>
		/// Zero flag.
		/// </summary>
		public bool ZF { get; set; }

		/// <summary>
		/// Overflow flag.
		/// </summary>
		public bool OF { get; set; }

		/// <summary>
		/// Sign flag.
		/// </summary>
		public bool SF { get; set; }

		/// <summary>
		/// Gets or sets value of stack pointer register.
		/// </summary>
		public uint StackPointer
		{
			get => GeneralRegisters[(int)Registers.SP];
			set
			{
				if (value < MinStack)
				{
					throw new StackOverflowException( "sponsored by stackoverflow.com" );
				}
				GeneralRegisters[(int)Registers.SP] = value;
			}
		}

		/// <summary>
		/// All valid commands.
		/// </summary>
		Command[] Commands { get; init; }

		/// <summary>
		/// If true, CPU will execute next command at <see cref="InstructionPointer"/> address. <br></br>
		/// After that, this flag wil be reset to <see langword="false"/>. <br></br>
		/// If set to false, CPU will move <see cref="InstructionPointer"/> to next command and execute it. <br></br>
		/// Use for jump commands.
		/// </summary>
		bool NextCommandOverriden { get; set; }

		/// <summary>
		/// Lowest possible address of stack pointer so it doesn't conflict with program memory and code.
		/// </summary>
		uint MinStack { get; init; }


		/// <summary>
		/// Initializes a new instance with pre-compiled memory, and determined entry point and stack pointer.
		/// </summary>
		/// <param name="memory"> Program memory, including code and data. </param>
		/// <param name="entryPoint"> Address of first instruction. </param>
		/// <param name="stackPointer"> Starting stack pointer. </param>
		public CPU( byte[] memory, uint entryPoint, uint stackPointer )
		{
			Commands = Command.Commands;
			Memory = memory;
			InstructionPointer = entryPoint;
			GeneralRegisters = new uint[RegistersCount];
			StackPointer = stackPointer;
			MinStack = stackPointer - StackReserve;
		}


		/// <summary>
		/// Returns name of register by specified index and size.
		/// </summary>
		/// <param name="index"> Index of register. </param>
		/// <param name="size"> Size of register. </param>
		/// <returns> Lowercase name of register. </returns>
		public string GetRegisterName( uint index, SizeMode size = SizeMode.FourBytes )
		{
			string prefix = size == SizeMode.FourBytes ? "e" : "";
			string suffix;
			if (index == (uint)Registers.SI || index == (uint)Registers.DI ||
				index == (uint)Registers.SP || index == (uint)Registers.BP)
			{
				suffix = size == SizeMode.TwoBytesHigher ? "h" : size == SizeMode.OneByte ? "l" : "";
			}
			else
			{
				suffix = size == SizeMode.TwoBytesHigher ? "h" : size == SizeMode.OneByte ? "l" : "x";
			}

			return $"{prefix}{((Registers)index).ToStringExt()}{suffix}";
		}


		/// <summary>
		/// Returns assembler code of next command.
		/// </summary>
		/// <returns></returns>
		public string GetNextCommandAsm()
		{
			return GetAsmForCommandAt( InstructionPointer, out _ );
		}


		/// <summary>
		/// Returns assembler code for command at specified address. Alse outputs total length of that command.
		/// </summary>
		/// <param name="memoryIndex"> Start address of command. </param>
		/// <param name="commandLength"> Length of that command. </param>
		/// <returns></returns>
		public string GetAsmForCommandAt( uint memoryIndex, out int commandLength )
		{
			uint commandIndex = (Memory[memoryIndex] & 0b_1111_1100u) >> 2;
			if (commandIndex >= Commands.Length)
			{
				commandLength = 4;
				return "invalid command";
			}
			SizeMode sizeMode = (SizeMode)((Memory[memoryIndex] & 0b_0000_0011u) >> 0);
			OperandsMode operandsMode = (OperandsMode)((Memory[memoryIndex + 1] & 0b_1100_0000u) >> 6);
			uint reg1 = (Memory[memoryIndex + 1] & 0b_0011_1000u) >> 3;
			uint op2Info = (Memory[memoryIndex + 1] & 0b_0000_0111u) >> 0;
			if (Commands[commandIndex].GetCode() == MovInstructionCode && sizeMode == SizeMode.FourBytes
				&& operandsMode == OperandsMode.RegImm && op2Info != 0)
			{
				commandLength = 8;
			}
			else
			{
				commandLength = 4;
			}
			return Commands[commandIndex].GetAsm( this, sizeMode, operandsMode, reg1, op2Info,
				Memory[memoryIndex + 2], Memory[memoryIndex + 3],
				BitConverter.ToUInt32( Memory, (int)memoryIndex + 4 ) );
		}


		public bool IsCommandTheEndCommand( uint memoryIndex )
		{
			uint commandIndex = (Memory[memoryIndex] & 0b_1111_1100u) >> 2;
			if (Commands[commandIndex].GetCode() == EndInstructionCode)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Executes next command and returns whether program continues, or if it has reached the end.
		/// </summary>
		/// <returns> <see langword="true"/> if executed command was not 'End' command, so execution should continue </returns>
		public bool DoNextCommand()
		{
			uint commandIndex = (Memory[InstructionPointer] & 0b_1111_1100u) >> 2;
			var command = Commands[commandIndex];
			if (command.GetCode().ToLower() == EndInstructionCode)
			{
				return false;
			}
			if (command.HasNoOperands())
			{
				command.ProcessNoOperands( this );
				MoveNext();
				return true;
			}
			SizeMode sizeMode = (SizeMode)((Memory[InstructionPointer] & 0b_0000_0011u) >> 0);
			OperandsMode operandsMode = (OperandsMode)((Memory[InstructionPointer + 1] & 0b_1100_0000u) >> 6);
			if (operandsMode == OperandsMode.RegReg)
			{
				uint reg1 = (Memory[InstructionPointer + 1] & 0b_0011_1000u) >> 3;
				if (command.HasOneOperand())
				{
					command.ProcessOneRegOperand( this, reg1, sizeMode );
				}
				else
				{
					uint reg2 = (Memory[InstructionPointer + 1] & 0b_0000_0111u) >> 0;
					command.ProcessRegReg( this, reg1, reg2, sizeMode );
				}
			}
			else if (operandsMode == OperandsMode.RegImm)
			{
				uint reg = (Memory[InstructionPointer + 1] & 0b_0011_1000u) >> 3;
				uint immediate;
				if (command.GetCode().ToLower() == MovInstructionCode && sizeMode == SizeMode.FourBytes
					&& (Memory[InstructionPointer + 1] & 0b_0000_0111u) != 0)
				{
					MoveNext();
					immediate = BitConverter.ToUInt32( Memory, (int)InstructionPointer );
				}
				else
				{
					immediate = BitConverter.ToUInt16( Memory, (int)(InstructionPointer + 2) );
				}
				if (command.HasOneOperand())
				{
					command.ProcessOneImmediateOperand( this, immediate );
				}
				else
				{
					command.ProcessRegImm( this, reg, immediate, sizeMode );
				}
			}
			else
			{
				uint scale = (Memory[InstructionPointer + 1] & 0b_0000_0111u) >> 0;
				uint address = CalculateAddress( scale, Memory[InstructionPointer + 2], Memory[InstructionPointer + 3] );
				if (command.HasOneOperand())
				{
					Commands[commandIndex].ProcessOneMemoryOperand( this, address, sizeMode );
				}
				else
				{
					uint reg = (Memory[InstructionPointer + 1] & 0b_0011_1000u) >> 3;
					if (operandsMode == OperandsMode.RegMem)
					{
						command.ProcessRegMem( this, reg, address, sizeMode );
					}
					else
					{
						command.ProcessMemReg( this, reg, address, sizeMode );
					}
				}
			}
			MoveNext();
			return true;
		}


		/// <summary>
		/// Returns bit fields of command at specified address.
		/// </summary>
		/// <param name="address"> Address of command. </param>
		/// <returns></returns>
		public (string bits, string desc)[] GetFieldsCommand( uint address )
		{
			List<(string bits, string desc)> fields = new List<(string bits, string desc)>();
			string bits = Convert.ToString( Memory[address], 2 ).PadLeft( 8, '0' );
			fields.Add( (bits[0..6], "Code") );
			var size = (SizeMode)(Memory[address] & 0b_0000_0011);
			fields.Add( (bits[6..], "Size " + size.SizeUint()) );
			string bits2 = Convert.ToString( Memory[address + 1], 2 ).PadLeft( 8, '0' );
			var op = (OperandsMode)((Memory[address + 1] & 0b_1100_0000) >> 6);
			fields.Add( (bits2[0..2], "Type\n" + op.Types()) );
			fields.Add( (bits2[2..5], "Reg 1\n" + GetRegisterName( (Memory[address + 1] & 0b_0011_1000u) >> 3, size )) );
			string bits3 = Convert.ToString( Memory[address + 2], 2 ).PadLeft( 8, '0' );
			string bits4 = Convert.ToString( Memory[address + 3], 2 ).PadLeft( 8, '0' );
			if (op == OperandsMode.RegReg)
			{
				fields.Add( (bits2[5..], "Reg 2\n" + GetRegisterName( Memory[address + 1] & 0b_0000_0111u, size )) );
				return fields.ToArray();
			}
			else if (op == OperandsMode.RegImm)
			{
				fields.Add( (bits2[5..], "32-bit const\n" + ((Memory[address + 1] & 0b_0000_0111u) != 0)) );
				if ((Memory[address + 1] & 0b_0000_0111u) != 0)
				{
					int c = BitConverter.ToInt32( Memory, (int)(address + 4) );
					fields.Add( (c.ToString(), "Const 32-bit") );
				}
				else
				{
					fields.Add( (bits3 + bits4, "Const 16-bit") );
				}
				return fields.ToArray();
			}
			else
			{
				uint scale = 1u << (int)(Memory[address + 1] & 0b_0000_0111u);
				fields.Add( (bits2[5..], "Scale\n" + scale) );
				bool useBase = (Memory[address + 2] & 0b_1000_0000) == 0;
				fields.Add( (bits3[0..1], "Use\nbase\nregister\n" + useBase) );
				fields.Add( (bits3[1..4], "Base\nregister\n" + GetRegisterName( (Memory[address + 2] & 0b_0111_0000u) >> 4, SizeMode.FourBytes )) );
				bool useIndex = (Memory[address + 2] & 0b_0000_1000) == 0;
				fields.Add( (bits3[4..5], "Use\nindex\n" + useIndex) );
				fields.Add( (bits3[5..], "Index\n" + GetRegisterName( Memory[address + 2] & 0b_0000_0111u, SizeMode.FourBytes )) );
				fields.Add( (bits4, "Displacement\n" + ((sbyte)Memory[address + 3]).ToString()) );
				return fields.ToArray();
			}
		}


		/// <summary>
		/// Overrides next command pointer.
		/// </summary>
		/// <param name="jumpTo"> Address of new next command. </param>
		internal void SetNextCommandPointer( uint jumpTo )
		{
			NextCommandOverriden = true;
			InstructionPointer = jumpTo;
		}


		/// <summary>
		/// Clears flags.
		/// </summary>
		internal void ClearFlags()
		{
			CF = ZF = OF = SF = false;
		}


		/// <summary>
		/// Outputs signed number into <see cref="OutputStream"/> if it's not <see langword="null"/>
		/// </summary>
		/// <param name="value"></param>
		internal void Output( int value )
		{
			OutputStream?.Write( Encoding.Unicode.GetBytes( value.ToString() ) );
			OutputStream?.Flush();
		}


		/// <summary>
		/// Outputs unsigned number into <see cref="OutputStream"/> if it's not <see langword="null"/>
		/// </summary>
		/// <param name="value"></param>
		internal void Output( uint value )
		{
			OutputStream?.Write( Encoding.Unicode.GetBytes( value.ToString() ) );
			OutputStream?.Flush();
		}


		/// <summary>
		/// Returns multiplier for scale field of address operand, instead if power of of 2.
		/// </summary>
		/// <param name="encodedScale"> Scale field of address oeprand, which is encoded as power of 2. </param>
		/// <returns> 2 to the power of <paramref name="encodedScale"/> </returns>
		internal uint GetActualScale( uint encodedScale )
		{
			return 1u << (int)encodedScale;
		}


		/// <summary>
		/// Returns string representation of address operand in command.
		/// </summary>
		/// <param name="scale"> Scale field of address. </param>
		/// <param name="b1"> Third byte (from the left) of command. </param>
		/// <param name="b0"> Last byte (from the left) of command. </param>
		/// <returns></returns>
		internal string GetAddressString( uint scale, byte b1, byte b0 )
		{
			uint reg1 = (b1 & 0b_1111_0000u) >> 4;
			bool useReg1 = reg1 < RegistersCount;
			uint reg2 = (b1 & 0b_0000_1111u) >> 0;
			bool useReg2 = reg2 < RegistersCount;
			uint actualScale = GetActualScale( scale );
			string str = "";
			if (useReg1)
			{
				str += GetRegisterName( reg1 );
				if (useReg2 || b0 != 0)
				{
					str += "+";
				}
			}
			if (useReg2)
			{
				str += GetRegisterName( reg2 );
				if (actualScale != 1)
				{
					str += "*" + actualScale;
				}
				if (b0 != 0)
				{
					str += "+";
				}
			}
			if (b0 != 0)
			{
				str += b0.ToString();
			}
			return str;
		}


		/// <summary>
		/// Returns number at specified offset of the stack.
		/// </summary>
		/// <param name="offset"> Offset from the top of stack. </param>
		/// <returns> Signed value of number in the stack. </returns>
		public int GetStackValue( int offset )
		{
			return BitConverter.ToInt32( Memory, (int)(StackPointer + offset) );
		}


		/// <summary>
		///	Returns an actual address encoded in operand of the command.
		/// </summary>
		/// <param name="scale"> Scale field of address. </param>
		/// <param name="b1"> Third byte (from the left) of command. </param>
		/// <param name="b0"> Last byte (from the left) of command. </param>
		/// <returns></returns>
		uint CalculateAddress( uint scale, byte b1, byte b0 )
		{
			uint reg1 = (b1 & 0b_1111_0000u) >> 4;
			bool useReg1 = reg1 < RegistersCount;
			uint reg2 = (b1 & 0b_0000_1111u) >> 0;
			bool useReg2 = reg2 < RegistersCount;
			uint actualScale = GetActualScale( scale );
			uint address = (uint)(sbyte)b0;
			if (useReg1)
			{
				address += GeneralRegisters[reg1];
			}
			if (useReg2)
			{
				address += actualScale * GeneralRegisters[reg2];
			}
			return address;
		}


		/// <summary>
		/// Moves <see cref="InstructionPointer"/> to the next command, taking into consideration <br></br>
		/// <see cref="NextCommandOverriden"/> flag, and resetting it if it's <see langword="true"/>
		/// </summary>
		void MoveNext()
		{
			if (NextCommandOverriden)
			{
				NextCommandOverriden = false;
			}
			else
			{
				InstructionPointer += 4;
			}
		}
	}
}
