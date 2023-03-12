namespace Emulator.Commands
{
	public class Jc : JumpCommands
	{
		protected override bool CheckFlagsToJump( CPU cpu )
		{
			return cpu.CF;
		}
	}
}
