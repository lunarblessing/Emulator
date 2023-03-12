namespace Emulator.Commands
{
	public abstract class JumpCommands : Command
	{

		public override string GetCode()
		{
			return GetType().Name.ToLower();
		}

		public override bool HasOneOperand()
		{
			return true;
		}

		public override void ProcessOneRegOperand( CPU cpu, uint reg, SizeMode size )
		{
			var address = cpu.GeneralRegisters[reg];
			ProcessOneMemoryOperand( cpu, address, size );
		}

		public override void ProcessOneImmediateOperand( CPU cpu, uint immediate )
		{
			var address = cpu.InstructionPointer + (short)immediate * 4;
			ProcessOneMemoryOperand( cpu, (uint)address, SizeMode.TwoBytes );
		}

		public override void ProcessOneMemoryOperand( CPU cpu, uint address, SizeMode size )
		{
			if (CheckFlagsToJump( cpu ))
			{
				cpu.SetNextCommandPointer( address );
			}
		}

		public override bool IsJumpCommand()
		{
			return true;
		}

		protected virtual bool CheckFlagsToJump( CPU cpu )
		{
			return false;
		}
	}
}
