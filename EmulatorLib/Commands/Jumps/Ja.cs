namespace Emulator.Commands
{
	public class Ja : JumpCommands
	{
		protected override bool CheckFlagsToJump( CPU cpu )
		{
			return !cpu.CF && !cpu.ZF;
		}
	}
}
