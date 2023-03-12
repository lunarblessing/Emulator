namespace Emulator.Commands
{
	public class Je : JumpCommands
	{
		protected override bool CheckFlagsToJump( CPU cpu )
		{
			return cpu.ZF;
		}
	}
}
