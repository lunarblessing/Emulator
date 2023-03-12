namespace Emulator.Commands
{
	public class Jbe : JumpCommands
	{
		protected override bool CheckFlagsToJump( CPU cpu )
		{
			return cpu.CF || cpu.ZF;
		}
	}
}
