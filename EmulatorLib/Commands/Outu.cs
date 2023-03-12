using System;

namespace Emulator.Commands
{
	public class Outu : Command
	{
		public override string GetCode()
		{
			return "outu";
		}

		public override bool HasOneOperand()
		{
			return true;
		}

		public override void ProcessOneRegOperand( CPU cpu, uint reg, SizeMode size )
		{
			if (size == SizeMode.OneByte)
			{
				cpu.Output( cpu.GeneralRegisters[reg] & 0x000000ffu );
			}
			else if (size == SizeMode.TwoBytes)
			{
				cpu.Output( (ushort)(cpu.GeneralRegisters[reg] & 0x0000ffffu) );
			}
			else if (size == SizeMode.TwoBytesHigher)
			{

			}
			else
			{
				cpu.Output( cpu.GeneralRegisters[reg] );
			}
		}

		public override void ProcessOneImmediateOperand( CPU cpu, uint immediate )
		{
			cpu.Output( immediate );
		}

		public override void ProcessOneMemoryOperand( CPU cpu, uint address, SizeMode size )
		{
			if (size == SizeMode.OneByte)
			{
				cpu.Output( cpu.Memory[address] );
			}
			else if (size == SizeMode.TwoBytes)
			{
				cpu.Output( BitConverter.ToUInt16( cpu.Memory, (int)address ) );
			}
			else if (size == SizeMode.TwoBytesHigher)
			{

			}
			else
			{
				cpu.Output( BitConverter.ToUInt32( cpu.Memory, (int)address ) );
			}
		}
	}
}
