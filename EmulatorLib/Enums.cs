namespace Emulator
{

	/// <summary>
	/// Possible core registers without specified size. Appear in the order they are encoded in commands.
	/// </summary>
	public enum Registers
	{
		// Should be sequential starting with 0
		A, B, C, D, DI, SI, BP, SP
	}

	/// <summary>
	/// Size of command or operands.
	/// </summary>
	public enum SizeMode
	{
		// If changing, change static register names order in CPU class (CPU.registersByIndex)
		FourBytes, TwoBytes, TwoBytesHigher, OneByte
	}

	/// <summary>
	/// Mode of operands in command.
	/// </summary>
	public enum OperandsMode
	{
		RegReg, RegImm, RegMem, MemReg
	}

	/// <summary>
	/// Extensions for CPU module.
	/// </summary>
	public static class Extensions
	{

		/// <summary>
		/// Returns lowercase core name of register without specified size.
		/// </summary>
		/// <param name="reg"></param>
		/// <returns> lowercase name of core register. </returns>
		public static string ToStringExt( this Registers reg )
		{
			return reg switch
			{
				Registers.A => "a",
				Registers.B => "b",
				Registers.C => "c",
				Registers.D => "d",
				Registers.DI => "di",
				Registers.SI => "si",
				Registers.BP => "bp",
				Registers.SP => "sp",
				_ => "invalid register",
			};
		}

		/// <summary>
		/// Returns number of bytes described by <see cref="SizeMode"/> enum.
		/// </summary>
		/// <param name="size"></param>
		/// <returns> Number of bytes required to keep operand with this <see cref="SizeMode"/>. </returns>
		public static uint SizeUint( this SizeMode size )
		{
			return size switch
			{
				SizeMode.FourBytes => 4,
				SizeMode.TwoBytes => 2,
				SizeMode.TwoBytesHigher => 2,
				SizeMode.OneByte => 1,
				_ => 4,
			};
		}

		/// <summary>
		/// Returns lowercase description of <see cref="OperandsMode"/> mode of command.
		/// </summary>
		/// <param name="size"></param>
		/// <returns> Lowercase description of mode </returns>
		public static string Types( this OperandsMode mode )
		{
			return mode switch
			{
				OperandsMode.RegReg => "reg-reg",
				OperandsMode.RegImm => "reg-const",
				OperandsMode.RegMem => "reg-address",
				OperandsMode.MemReg => "address-reg",
				_ => "invalid mode",
			};
		}
	}
}
