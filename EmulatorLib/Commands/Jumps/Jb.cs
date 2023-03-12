namespace Emulator.Commands
{
	public class Jb : JumpCommands
	{
		protected override bool CheckFlagsToJump( CPU cpu )
		{
			return cpu.CF;
		}
	}
}
