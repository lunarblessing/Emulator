namespace Emulator.Commands
{
	public class Jae : JumpCommands
	{
		protected override bool CheckFlagsToJump( CPU cpu )
		{
			return !cpu.CF;
		}
	}
}
