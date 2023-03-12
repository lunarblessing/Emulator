namespace Emulator.Assembler
{
	public class Operand
	{
		public bool IsRegister { get; set; }
		public bool IsAddress { get; set; }
		public bool IsImmediate { get; set; }
		public uint RegisterIndex { get; set; }
		public AddressNode? AddressNode { get; set; }
		public SimpleValueNode? Immediate { get; set; }
	}
}
