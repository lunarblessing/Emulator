using System;

namespace Emulator.Commands
{
	public class Mov : Command
	{
		public override string GetCode()
		{
			return CPU.MovInstructionCode;
		}

		public override void ProcessRegReg( CPU cpu, uint reg1, uint reg2, SizeMode size )
		{
			if (size == SizeMode.OneByte)
			{
				cpu.GeneralRegisters[reg1] = cpu.GeneralRegisters[reg1] & 0xFFFFFF00u | cpu.GeneralRegisters[reg2] & 0x000000FFu;
			}
			else if (size == SizeMode.TwoBytes)
			{
				cpu.GeneralRegisters[reg1] = cpu.GeneralRegisters[reg1] & 0xFFFF0000u | cpu.GeneralRegisters[reg2] & 0x0000FFFFu;
			}
			else if (size == SizeMode.TwoBytesHigher)
			{

			}
			else
			{
				cpu.GeneralRegisters[reg1] = cpu.GeneralRegisters[reg2];
			}
		}

		public override void ProcessRegImm( CPU cpu, uint reg, uint immediate, SizeMode size )
		{
			if (size == SizeMode.OneByte)
			{
				cpu.GeneralRegisters[reg] = cpu.GeneralRegisters[reg] & 0xFFFFFF00u | immediate & 0x00FFu;
			}
			else if (size == SizeMode.TwoBytes)
			{
				cpu.GeneralRegisters[reg] = cpu.GeneralRegisters[reg] & 0xFFFF0000u | immediate & 0x0000FFFFu;
			}
			else if (size == SizeMode.TwoBytesHigher)
			{
				cpu.GeneralRegisters[reg] = cpu.GeneralRegisters[reg] & 0x0000FFFFu | immediate << 16;
			}
			else
			{
				cpu.GeneralRegisters[reg] = immediate;
			}
		}

		public override void ProcessRegMem( CPU cpu, uint reg, uint address, SizeMode size )
		{
			if (size == SizeMode.OneByte)
			{
				cpu.GeneralRegisters[reg] = cpu.GeneralRegisters[reg] & 0xFFFFFF00u | cpu.Memory[address];
			}
			else if (size == SizeMode.TwoBytes)
			{
				cpu.GeneralRegisters[reg] = cpu.GeneralRegisters[reg] & 0xFFFF0000u | BitConverter.ToUInt16( cpu.Memory, (int)address );
			}
			else if (size == SizeMode.TwoBytesHigher)
			{

			}
			else
			{
				cpu.GeneralRegisters[reg] = BitConverter.ToUInt32( cpu.Memory, (int)address );
			}
		}

		public override void ProcessMemReg( CPU cpu, uint reg, uint address, SizeMode size )
		{
			if (size == SizeMode.OneByte)
			{
				cpu.Memory[address] = (byte)cpu.GeneralRegisters[reg];
			}
			else if (size == SizeMode.TwoBytes)
			{
				cpu.Memory[address] = (byte)cpu.GeneralRegisters[reg];
				cpu.Memory[address + 1] = (byte)(cpu.GeneralRegisters[reg] >> 8);
			}
			else if (size == SizeMode.TwoBytesHigher)
			{

			}
			else
			{
				cpu.Memory[address] = (byte)cpu.GeneralRegisters[reg];
				cpu.Memory[address + 1] = (byte)(cpu.GeneralRegisters[reg] >> 8);
				cpu.Memory[address + 2] = (byte)(cpu.GeneralRegisters[reg] >> 16);
				cpu.Memory[address + 3] = (byte)(cpu.GeneralRegisters[reg] >> 24);
			}
		}
	}
}
