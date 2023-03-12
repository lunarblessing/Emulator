namespace Emulator.Commands
{
	public class Jl : JumpCommands
	{
		protected override bool CheckFlagsToJump( CPU cpu )
		{
			return cpu.SF != cpu.OF;
		}
	}
}
