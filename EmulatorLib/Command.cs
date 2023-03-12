using System;
using System.Linq;
using System.Reflection;
using Emulator.Commands;

namespace Emulator
{

	/// <summary>
	/// Base abstract class for CPU commands. Commands are created automatically for each non-abstract subclass,
	/// and are available from static <see cref="Commands"/>.
	/// </summary>
	public abstract class Command
	{

		////////// Static

		/// <summary>
		/// Singleton array of every possible command.
		/// </summary>
		static Command[]? instances;

		/// <summary>
		/// Array of all implemented commands. Always starts with 'End' command which halts execution,
		/// so array of zeroes would be interpreted as End command.
		/// </summary>
		public static Command[] Commands
		{
			get => instances ?? InitializeCommands();
		}


		/// <summary>
		/// Initializes <see cref="instances"/> with instances of every non-abstract subclass. <br></br>
		/// Returns created array.
		/// </summary>
		static Command[] InitializeCommands()
		{
			var derivedClasses = from type in Assembly.GetAssembly( typeof( Command ) ).GetTypes()
								 where type.IsClass && !type.IsAbstract && type.IsSubclassOf( typeof( Command ) )
								 select type;
			instances = new Command[derivedClasses.Count()];
			instances[0] = new End(); // first command is alwayd End command
			int i = 1;
			foreach (var type in derivedClasses)
			{
				if (type == typeof( End ))
				{
					continue;
				}
				instances[i] = (Command)Activator.CreateInstance( type );
				i++;
			}
			return instances;
		}


		/// <summary>
		/// Checks if command with specified code exists in <see cref="Commands"/>, case-insensitive.
		/// </summary>
		/// <param name="code"> Case-insensitive code of command. </param>
		/// <returns></returns>
		public static bool CommandCodeExists( string code )
		{
			return Commands.Any( comm => comm.GetCode() == code.ToLower() );
		}


		/// <summary>
		/// Returns tuple of possible overrides of command with specified code. <br></br>
		/// If command has no overrides with some operands count, returns <see langword="null"/> in its place.
		/// </summary>
		/// <param name="code"> Case-insensitive code of command. </param>
		/// <returns> Tuple of command overrides with no operands, one operand and with two operands. </returns>
		public static (Command? noOp, Command? oneOp, Command? twoOp) GetOverridesOfCommand( string code )
		{
			Command? noOP = null, oneOp = null, twoOp = null;
			foreach (var command in Commands)
			{
				if (command.GetCode() == code.ToLower())
				{
					if (command.HasNoOperands())
					{
						noOP = command;
					}
					else if (command.HasOneOperand())
					{
						oneOp = command;
					}
					else
					{
						twoOp = command;
					}
				}
			}
			return (noOP, oneOp, twoOp);
		}


		/// <summary>
		/// Returns index of command in <see cref="Commands"/>.
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public static int GetCommandIndex( Command command )
		{
			return Array.IndexOf( Commands, command );
		}


		////////// Non-static and not virtual methods.


		/// <summary>
		/// Returns assembler string for command with specified parameters.
		/// </summary>
		/// <param name="cpu"> CPU, used to access register and address strings. </param>
		/// <param name="size"> <see cref="SizeMode"/> of this command. </param>
		/// <param name="opMode"> <see cref="OperandsMode"/> of this command. </param>
		/// <param name="reg1"> Index of first register in this command <br></br> 
		/// (or the only register, if command has only one register) </param>
		/// <param name="op2Info"> Scale field of address, or index of second register depending on operands. </param>
		/// <param name="b1"> Third byte (from the left) of command </param>
		/// <param name="b0"> Last byte (from the left) of command </param>
		/// <param name="nextUint"> Next 4 bytes value, used for commands which use 4-byte constants. </param>
		/// <returns> Assembler string of command </returns>
		public string GetAsm( CPU cpu, SizeMode size, OperandsMode opMode,
			uint reg1, uint op2Info, byte b1, byte b0, uint nextUint )
		{
			if (HasNoOperands())
			{
				return GetCode();
			}
			if (HasOneOperand())
			{
				if (opMode == OperandsMode.RegReg)
				{
					// register is the only operand
					return $"{GetCode()} {cpu.GetRegisterName( reg1, size )}";
				}
				if (opMode == OperandsMode.RegImm)
				{
					// constant is the only operand
					return $"{GetCode()} {(short)(b1 + b0 * 256)}";
				}
				// address is the only operand
				return $"{GetCode()} [{cpu.GetAddressString( op2Info, b1, b0 )}]";
			}
			// 2 operands
			if (opMode == OperandsMode.RegReg)
			{
				return $"{GetCode()} {cpu.GetRegisterName( reg1, size )}, {cpu.GetRegisterName( op2Info, size )}";
			}
			if (opMode == OperandsMode.RegImm)
			{
				if (GetCode() == CPU.MovInstructionCode && size == SizeMode.FourBytes && op2Info != 0)
				{
					return $"{GetCode()} {cpu.GetRegisterName( reg1, size )}, {(int)nextUint}";
				}
				return $"{GetCode()} {cpu.GetRegisterName( reg1, size )}, {(short)(b1 + b0 * 256)}";
			}
			if (opMode == OperandsMode.RegMem)
			{
				return $"{GetCode()} {cpu.GetRegisterName( reg1, size )}, [{cpu.GetAddressString( op2Info, b1, b0 )}]";
			}
			// MemReg
			return $"{GetCode()} [{cpu.GetAddressString( op2Info, b1, b0 )}], {cpu.GetRegisterName( reg1, size )}";
		}


		/// <summary>
		/// Returns string representation of this command override - <br></br>
		/// includes code of command and number of operands. <br></br> 
		/// Use <see cref="GetAsm(CPU, SizeMode, OperandsMode, uint, uint, byte, byte, uint)"/>
		/// for string describing command with exact parameters.
		/// </summary>
		/// <returns> String consisting of command code and number of operands of this override. </returns>
		public override string ToString()
		{
			return $"{GetCode()} {(HasOneOperand() ? "1 op" : HasNoOperands() ? "0 op" : "2 op")}";
		}


		////////// Virtual methods


		/// <summary>
		/// Returns lowercawe code of current command.
		/// </summary>
		/// <returns> Lowercase code of command. </returns>
		public abstract string GetCode();


		/// <summary>
		/// Returns whether this command override has two operands. <para></para> 
		/// When overriding: doens;t need to be overriden, as by default result depends on <br></br>
		/// <see cref="HasOneOperand"/> and <see cref="HasNoOperands"/>
		/// </summary>
		/// <returns></returns>
		public virtual bool HasTwoOperands()
		{
			return !HasNoOperands() && !HasOneOperand();
		}


		/// <summary>
		/// Returns whether this command override has one operand. <para></para> 
		/// When overriding: override to return true only if command has one operand.
		/// </summary>
		/// <returns></returns>
		public virtual bool HasOneOperand()
		{
			return false;
		}


		/// <summary>
		/// Returns whether this command override has no operands. <para></para> 
		/// When overriding: override to return true only if command has no operands.
		/// </summary>
		/// <returns></returns>
		public virtual bool HasNoOperands()
		{
			return false;
		}


		/// <summary>
		/// Processes command with no operands. <br></br>
		/// If this command override has any operands, this does nothing.
		/// </summary>
		/// <param name="cpu"></param>
		public virtual void ProcessNoOperands( CPU cpu )
		{

		}


		/// <summary>
		/// Processes command with one register operand. <br></br>
		/// If this command override has other count of operands, this does nothing.
		/// </summary>
		/// <param name="cpu"></param>
		/// <param name="reg"> Index of operand register. </param>
		/// <param name="size"></param>
		public virtual void ProcessOneRegOperand( CPU cpu, uint reg, SizeMode size )
		{

		}


		/// <summary>
		/// Processes command with two register operands. <br></br>
		/// If this command override has other count of operands, this does nothing.
		/// </summary>
		/// <param name="cpu"></param>
		/// <param name="reg1"> Index of first operand register. </param>
		/// <param name="reg2"> Index of second operand register. </param>
		/// <param name="size"></param>
		public virtual void ProcessRegReg( CPU cpu, uint reg1, uint reg2, SizeMode size )
		{

		}


		/// <summary>
		/// Processes command with one constant operand. <br></br>
		/// If this command override has other count of operands, this does nothing.
		/// </summary>
		/// <param name="cpu"></param>
		/// <param name="immediate"> Value of constant. <br></br>
		/// Can be max 2 bytes unless command is allowed to work with 4-bytes constant. </param>
		public virtual void ProcessOneImmediateOperand( CPU cpu, uint immediate )
		{

		}


		/// <summary>
		/// Processes command with two operands - register and constant. <br></br>
		/// If this command override has other count of operands, this does nothing.
		/// </summary>
		/// <param name="cpu"></param>
		/// <param name="reg"> Index of register operand. </param>
		/// <param name="immediate"> Value of constant. <br></br>
		/// Can be max 2 bytes unless command is allowed to work with 4-bytes constant. </param>
		/// <param name="size"></param>
		public virtual void ProcessRegImm( CPU cpu, uint reg, uint immediate, SizeMode size )
		{

		}


		/// <summary>
		/// Processes command with one memory address operand. <br></br>
		/// If this command override has other count of operands, this does nothing.
		/// </summary>
		/// <param name="cpu"></param>
		/// <param name="address"> Calculated address of operand. </param>
		/// <param name="size"></param>
		public virtual void ProcessOneMemoryOperand( CPU cpu, uint address, SizeMode size )
		{

		}


		/// <summary>
		/// Processes command with two operands - register and memory address. <br></br>
		/// If this command override has other count of operands, this does nothing.
		/// </summary>
		/// <param name="cpu"></param>
		/// <param name="reg"> Index of register operand. </param>
		/// <param name="address"> Calculated address of second operand. </param>
		/// <param name="size"></param>
		public virtual void ProcessRegMem( CPU cpu, uint reg, uint address, SizeMode size )
		{

		}


		/// <summary>
		/// Processes command with two operands - memory address and register. <br></br>
		/// If this command override has other count of operands, this does nothing.
		/// </summary>
		/// <param name="cpu"></param>
		/// <param name="address"> Calculated address of first operand. </param>
		/// <param name="reg"> Index of register operand. </param>
		/// <param name="size"></param>
		public virtual void ProcessMemReg( CPU cpu, uint reg, uint address, SizeMode size )
		{

		}


		/// <summary>
		/// Returns whether this command is jump command.
		/// </summary>
		/// <returns></returns>
		public virtual bool IsJumpCommand()
		{
			return false;
		}
	}
}
