namespace Emulator.Commands
{
	public class Jmp : JumpCommands
	{
		protected override bool CheckFlagsToJump( CPU cpu )
		{
			return true;
		}
	}
}
