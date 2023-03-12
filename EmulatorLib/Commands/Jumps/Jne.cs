namespace Emulator.Commands
{
	public class Jne : JumpCommands
	{
		protected override bool CheckFlagsToJump( CPU cpu )
		{
			return !cpu.ZF;
		}
	}
}
