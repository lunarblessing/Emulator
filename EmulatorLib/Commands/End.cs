namespace Emulator.Commands
{
	public class End : Command
	{
		public override string GetCode()
		{
			return CPU.EndInstructionCode;
		}

		public override bool HasNoOperands()
		{
			return true;
		}

		public override void ProcessNoOperands( CPU cpu )
		{
			
		}
	}
}
