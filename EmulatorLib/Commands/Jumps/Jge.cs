namespace Emulator.Commands
{
	public class Jge : JumpCommands
	{
		protected override bool CheckFlagsToJump( CPU cpu )
		{
			return cpu.SF == cpu.OF;
		}
	}
}
