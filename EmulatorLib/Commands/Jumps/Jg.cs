namespace Emulator.Commands
{
	public class Jg : JumpCommands
	{
		protected override bool CheckFlagsToJump( CPU cpu )
		{
			return !cpu.ZF && cpu.SF == cpu.OF;
		}
	}
}
