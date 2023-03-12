namespace Emulator.Commands
{
	public unsafe class Push : Command
	{
		public override string GetCode()
		{
			return "push";
		}

		public override bool HasOneOperand()
		{
			return true;
		}

		public override void ProcessOneRegOperand( CPU cpu, uint reg, SizeMode size )
		{
			uint value;
			if (size == SizeMode.OneByte)
			{
				value = (byte)cpu.GeneralRegisters[reg];
			}
			else if (size == SizeMode.TwoBytes)
			{
				value = (ushort)cpu.GeneralRegisters[reg];
			}
			else if (size == SizeMode.TwoBytesHigher)
			{
				value = (ushort)cpu.GeneralRegisters[reg];
			}
			else
			{
				value = cpu.GeneralRegisters[reg];
			}
			cpu.Memory[cpu.GeneralRegisters[(int)Registers.SP] - 4] = (byte)value;
			cpu.Memory[cpu.GeneralRegisters[(int)Registers.SP] - 3] = (byte)(value >> 8);
			cpu.Memory[cpu.GeneralRegisters[(int)Registers.SP] - 2] = (byte)(value >> 16);
			cpu.Memory[cpu.GeneralRegisters[(int)Registers.SP] - 1] = (byte)(value >> 24);
			cpu.StackPointer -= 4;
		}

		public override void ProcessOneImmediateOperand( CPU cpu, uint immediate )
		{
			cpu.Memory[cpu.GeneralRegisters[(int)Registers.SP] - 4] = (byte)immediate;
			cpu.Memory[cpu.GeneralRegisters[(int)Registers.SP] - 3] = (byte)(immediate >> 8);
			cpu.Memory[cpu.GeneralRegisters[(int)Registers.SP] - 2] = (byte)(immediate >> 16);
			cpu.Memory[cpu.GeneralRegisters[(int)Registers.SP] - 1] = (byte)(immediate >> 24);
			cpu.StackPointer -= 4;
		}

		public override void ProcessOneMemoryOperand( CPU cpu, uint address, SizeMode size )
		{
			uint value;
			fixed (byte* memPtr = &cpu.Memory[address])
			{
				if (size == SizeMode.OneByte)
				{
					value = *memPtr;
				}
				else if (size == SizeMode.TwoBytes)
				{
					value = *(ushort*)memPtr;
				}
				else if (size == SizeMode.TwoBytesHigher)
				{
					value = *(ushort*)memPtr;
				}
				else
				{
					value = *(uint*)memPtr;
				}
			}
			cpu.Memory[cpu.GeneralRegisters[(int)Registers.SP] - 4] = (byte)value;
			cpu.Memory[cpu.GeneralRegisters[(int)Registers.SP] - 3] = (byte)(value >> 8);
			cpu.Memory[cpu.GeneralRegisters[(int)Registers.SP] - 2] = (byte)(value >> 16);
			cpu.Memory[cpu.GeneralRegisters[(int)Registers.SP] - 1] = (byte)(value >> 24);
			cpu.StackPointer -= 4;
		}
	}
}
