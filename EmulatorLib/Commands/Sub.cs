using System;

namespace Emulator.Commands
{
	public class Sub : Command
	{
		public override string GetCode()
		{
			return "sub";
		}

		public override void ProcessRegReg( CPU cpu, uint reg1, uint reg2, SizeMode size )
		{
			if (size == SizeMode.OneByte)
			{
				var value = (byte)((byte)cpu.GeneralRegisters[reg1] - (byte)cpu.GeneralRegisters[reg2]);
				cpu.GeneralRegisters[reg1] = cpu.GeneralRegisters[reg1] & 0xFFFFFF00u | value;
			}
			else if (size == SizeMode.TwoBytes)
			{
				var value = (ushort)((ushort)cpu.GeneralRegisters[reg1] - (ushort)cpu.GeneralRegisters[reg2]);
				cpu.GeneralRegisters[reg1] = cpu.GeneralRegisters[reg1] & 0xFFFF0000u | value;
			}
			else if (size == SizeMode.TwoBytesHigher)
			{

			}
			else
			{
				cpu.GeneralRegisters[reg1] -= cpu.GeneralRegisters[reg2];
			}
		}

		public override void ProcessRegImm( CPU cpu, uint reg, uint immediate, SizeMode size )
		{
			if (size == SizeMode.OneByte)
			{
				var value = (byte)((byte)cpu.GeneralRegisters[reg] - (byte)immediate);
				cpu.GeneralRegisters[reg] = cpu.GeneralRegisters[reg] & 0xFFFFFF00u | value;
			}
			else if (size == SizeMode.TwoBytes)
			{
				var value = (ushort)((ushort)cpu.GeneralRegisters[reg] - (ushort)immediate);
				cpu.GeneralRegisters[reg] = cpu.GeneralRegisters[reg] & 0xFFFF0000u | value;
			}
			else if (size == SizeMode.TwoBytesHigher)
			{

			}
			else
			{
				cpu.GeneralRegisters[reg] -= immediate;
			}
		}

		public override void ProcessRegMem( CPU cpu, uint reg, uint address, SizeMode size )
		{
			if (size == SizeMode.OneByte)
			{
				var value = (byte)((byte)cpu.GeneralRegisters[reg] - cpu.Memory[address]);
				cpu.GeneralRegisters[reg] = cpu.GeneralRegisters[reg] & 0xFFFFFF00u | value;
			}
			else if (size == SizeMode.TwoBytes)
			{
				var value = (ushort)((ushort)cpu.GeneralRegisters[reg] - BitConverter.ToUInt16( cpu.Memory, (int)address ));
				cpu.GeneralRegisters[reg] = cpu.GeneralRegisters[reg] & 0xFFFF0000u | value;
			}
			else if (size == SizeMode.TwoBytesHigher)
			{

			}
			else
			{
				cpu.GeneralRegisters[reg] -= BitConverter.ToUInt32( cpu.Memory, (int)address );
			}
		}

		public override void ProcessMemReg( CPU cpu, uint reg, uint address, SizeMode size )
		{
			if (size == SizeMode.OneByte)
			{
				cpu.Memory[address] -= (byte)cpu.GeneralRegisters[reg];
			}
			else if (size == SizeMode.TwoBytes)
			{
				var value = (ushort)(BitConverter.ToUInt16( cpu.Memory, (int)address ) - (ushort)cpu.GeneralRegisters[reg]);
				cpu.Memory[address] = (byte)value;
				cpu.Memory[address + 1] = (byte)(value >> 8);
			}
			else if (size == SizeMode.TwoBytesHigher)
			{

			}
			else
			{
				var value = BitConverter.ToUInt32( cpu.Memory, (int)address ) - cpu.GeneralRegisters[reg];
				cpu.Memory[address] = (byte)value;
				cpu.Memory[address + 1] = (byte)(value >> 8);
				cpu.Memory[address + 2] = (byte)(value >> 16);
				cpu.Memory[address + 3] = (byte)(value >> 24);
			}
		}
	}
}
