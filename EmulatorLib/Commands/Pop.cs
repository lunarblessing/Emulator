namespace Emulator.Commands
{
	public unsafe class Pop : Command
	{
		public override string GetCode()
		{
			return "pop";
		}

		public override bool HasOneOperand()
		{
			return true;
		}

		public override void ProcessOneRegOperand( CPU cpu, uint reg, SizeMode size )
		{
			fixed (byte* memPtr = &cpu.Memory[cpu.StackPointer])
			{
				if (size == SizeMode.OneByte)
				{
					cpu.GeneralRegisters[reg] = cpu.GeneralRegisters[reg] & 0xFFFFFF00 | *memPtr;
				}
				else if (size == SizeMode.TwoBytes)
				{
					cpu.GeneralRegisters[reg] = cpu.GeneralRegisters[reg] & 0xFFFF0000 | *(ushort*)memPtr;
				}
				else if (size == SizeMode.TwoBytesHigher)
				{

				}
				else
				{
					cpu.GeneralRegisters[reg] = *(uint*)memPtr;
				}
			}
			cpu.StackPointer += 4;
		}

		public override void ProcessOneImmediateOperand( CPU cpu, uint immediate )
		{
			var amountOfBytes = immediate - immediate % 4;
			cpu.StackPointer += amountOfBytes;
		}

		public override void ProcessOneMemoryOperand( CPU cpu, uint address, SizeMode size )
		{
			fixed (byte* memPtr = &cpu.Memory[address])
			{
				if (size == SizeMode.OneByte)
				{
					*memPtr = cpu.Memory[cpu.StackPointer];
				}
				else if (size == SizeMode.TwoBytes)
				{
					*memPtr = cpu.Memory[cpu.StackPointer];
					*(memPtr + 1) = cpu.Memory[cpu.StackPointer + 1];
				}
				else if (size == SizeMode.TwoBytesHigher)
				{

				}
				else
				{
					*memPtr = cpu.Memory[cpu.StackPointer];
					*(memPtr + 1) = cpu.Memory[cpu.StackPointer + 1];
					*(memPtr + 2) = cpu.Memory[cpu.StackPointer + 2];
					*(memPtr + 3) = cpu.Memory[cpu.StackPointer + 3];
				}
			}
			cpu.StackPointer += 4;
		}
	}
}
