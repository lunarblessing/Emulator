namespace Emulator.Commands
{
	public unsafe class Cmp : Command
	{
		public override string GetCode()
		{
			return "cmp";
		}

		void CompareBytes( byte b1, byte b2, CPU cpu )
		{
			var result = (byte)(b1 - b2);
			if (b2 > b1)
			{
				cpu.CF = true;
			}
			if ((b1 & 0b_1000_0000u) != (result & 0b_1000_0000u))
			{
				cpu.OF = true;
			}
			if ((result & 0b_1000_0000u) != 0)
			{
				cpu.SF = true;
			}
			if (result == 0)
			{
				cpu.ZF = true;
			}
		}

		void CompareShorts( ushort s1, ushort s2, CPU cpu )
		{
			var result = (ushort)(s1 - s2);
			if (s2 > s1)
			{
				cpu.CF = true;
			}
			if ((s1 & 0x_80_00u) != (result & 0x_80_00u))
			{
				cpu.OF = true;
			}
			if ((result & 0x_80_00u) != 0)
			{
				cpu.SF = true;
			}
			if (result == 0)
			{
				cpu.ZF = true;
			}
		}

		void CompareInts( uint i1, uint i2, CPU cpu )
		{
			var result = i1 - i2;
			if (i2 > i1)
			{
				cpu.CF = true;
			}
			if ((i1 & 0x_8000_0000u) != (i2 & 0x_8000_0000u) && ((i1 & 0x_8000_0000u) != (result & 0x_8000_0000u)))
			{
				cpu.OF = true;
			}
			if ((result & 0x_8000_0000u) != 0)
			{
				cpu.SF = true;
			}
			if (result == 0)
			{
				cpu.ZF = true;
			}
		}

		public override void ProcessRegReg( CPU cpu, uint reg1, uint reg2, SizeMode size )
		{
			cpu.ClearFlags();
			if (size == SizeMode.OneByte)
			{
				CompareBytes( (byte)cpu.GeneralRegisters[reg1], (byte)cpu.GeneralRegisters[reg2], cpu );
			}
			else if (size == SizeMode.TwoBytes)
			{
				CompareShorts( (ushort)cpu.GeneralRegisters[reg1], (ushort)cpu.GeneralRegisters[reg2], cpu );
			}
			else if (size == SizeMode.TwoBytesHigher)
			{

			}
			else
			{
				CompareInts( cpu.GeneralRegisters[reg1], cpu.GeneralRegisters[reg2], cpu );
			}
		}

		public override void ProcessRegImm( CPU cpu, uint reg, uint immediate, SizeMode size )
		{
			cpu.ClearFlags();
			if (size == SizeMode.OneByte)
			{
				CompareBytes( (byte)cpu.GeneralRegisters[reg], (byte)immediate, cpu );
			}
			else if (size == SizeMode.TwoBytes)
			{
				CompareShorts( (ushort)cpu.GeneralRegisters[reg], (ushort)immediate, cpu );
			}
			else if (size == SizeMode.TwoBytesHigher)
			{

			}
			else
			{
				CompareInts( cpu.GeneralRegisters[reg], immediate, cpu );
			}
		}

		public override void ProcessRegMem( CPU cpu, uint reg, uint address, SizeMode size )
		{
			cpu.ClearFlags();
			fixed (byte* memPtr = &cpu.Memory[address])
			{
				if (size == SizeMode.OneByte)
				{
					CompareBytes( (byte)cpu.GeneralRegisters[reg], *memPtr, cpu );
				}
				else if (size == SizeMode.TwoBytes)
				{
					CompareShorts( (ushort)cpu.GeneralRegisters[reg], *(ushort*)memPtr, cpu );
				}
				else if (size == SizeMode.TwoBytesHigher)
				{

				}
				else
				{
					CompareInts( cpu.GeneralRegisters[reg], *(uint*)memPtr, cpu );
				}
			}

		}

		public override void ProcessMemReg( CPU cpu, uint reg, uint address, SizeMode size )
		{
			cpu.ClearFlags();
			fixed (byte* memPtr = &cpu.Memory[address])
			{
				if (size == SizeMode.OneByte)
				{
					CompareBytes( *memPtr, (byte)cpu.GeneralRegisters[reg], cpu );
				}
				else if (size == SizeMode.TwoBytes)
				{
					CompareShorts( *(ushort*)memPtr, (ushort)cpu.GeneralRegisters[reg], cpu );
				}
				else if (size == SizeMode.TwoBytesHigher)
				{

				}
				else
				{
					CompareInts( *(uint*)memPtr, cpu.GeneralRegisters[reg], cpu );
				}
			}
		}
	}
}
