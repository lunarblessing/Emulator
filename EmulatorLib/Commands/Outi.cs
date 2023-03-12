using System;

namespace Emulator.Commands
{
	public class Outi : Command
	{
		public override string GetCode()
		{
			return "outi";
		}

		public override bool HasOneOperand()
		{
			return true;
		}

		public override void ProcessOneRegOperand( CPU cpu, uint reg, SizeMode size )
		{
			if (size == SizeMode.OneByte)
			{
				cpu.Output( (sbyte)(cpu.GeneralRegisters[reg] & 0x000000ffu) );
			}
			else if (size == SizeMode.TwoBytes)
			{
				cpu.Output( (short)(cpu.GeneralRegisters[reg] & 0x0000ffffu) );
			}
			else if (size == SizeMode.TwoBytesHigher)
			{

			}
			else
			{
				cpu.Output( (int)cpu.GeneralRegisters[reg] );
			}
		}

		public override void ProcessOneImmediateOperand( CPU cpu, uint immediate )
		{
			cpu.Output( (int)immediate );
		}

		public override void ProcessOneMemoryOperand( CPU cpu, uint address, SizeMode size )
		{
			if (size == SizeMode.OneByte)
			{
				cpu.Output( (sbyte)cpu.Memory[address] );
			}
			else if (size == SizeMode.TwoBytes)
			{
				cpu.Output( BitConverter.ToInt16( cpu.Memory, (int)address ) );
			}
			else if (size == SizeMode.TwoBytesHigher)
			{

			}
			else
			{
				cpu.Output( BitConverter.ToInt32( cpu.Memory, (int)address ) );
			}
		}
	}
}
