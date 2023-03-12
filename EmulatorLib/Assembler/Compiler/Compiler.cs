using System.Collections.Generic;
using System.Linq;

namespace Emulator.Assembler.Compiler
{
	public unsafe class Compiler
	{
		public ParsingError? Error { get; private set; }

		Dictionary<string, long> ConstTable { get; set; } = new Dictionary<string, long>();
		Dictionary<string, VarWithBaseAddress> VarTable { get; set; } = new Dictionary<string, VarWithBaseAddress>();
		Dictionary<string, uint> LabelsTable { get; set; } = new Dictionary<string, uint>();
		List<PendingLabel> UnresolvedJumps { get; set; } = new List<PendingLabel>();

		uint StartDataAddress { get; set; } = CPU.SmallestAddress;
		uint NextDataAddress { get; set; }
		uint StartCodeAddress { get; set; }
		uint NextCodeAddress { get; set; }
		uint StackStartPointer { get; set; }
		byte[] MemoryContent { get; set; }

		public bool ProcessRoot( RootNode root )
		{
			NextDataAddress = StartDataAddress;
			if (!FillConstants( root.Children.OfType<ConstDeclaration>() ))
			{
				return false;
			}
			if (!EnsureMax1CodeSection( root ))
			{
				return false;
			}
			var codeSection = GetCodeSection( root );
			var sections = root.Children.OfType<SectionNode>();
			foreach (var section in sections)
			{
				if (section == codeSection)
				{
					continue;
				}
				if (!CalculateVarAddresses( section.Children.OfType<VarDeclaration>() ))
				{
					return false;
				}
			}
			if (codeSection == null)
			{
				Error = "No code section";
				return true;
			}
			StartCodeAddress = AlignAddressToSize( NextDataAddress, 1024 );
			NextCodeAddress = StartCodeAddress;
			MemoryContent = new byte[StartCodeAddress + codeSection.Children.Count * 8 + CPU.StackReserve + 1024];
			if (!EncodeCommands( codeSection.Children.OfType<CommandNode>() ))
			{
				return false;
			}
			if (!FillVarValues())
			{
				return false;
			}
			var stackMinAddress = NextCodeAddress + 512;
			StackStartPointer = stackMinAddress + CPU.StackReserve;
			return true;
		}

		public CPU StartCPU()
		{
			return new CPU( MemoryContent, StartCodeAddress, StackStartPointer );
		}

		bool EnsureMax1CodeSection( RootNode root )
		{
			SectionNode? codeSection = null;
			foreach (var child in root.Children)
			{
				if (child is SectionNode section && section.Name == "code")
				{
					if (codeSection == null)
					{
						codeSection = section;
					}
					else
					{
						Error = "More than 1 code section";
						return false;
					}
				}
			}
			return true;
		}

		SectionNode? GetCodeSection( RootNode root )
		{
			foreach (var child in root.Children)
			{
				if (child is SectionNode section && section.Name == "code")
				{
					return section;
				}
			}
			return null;
		}

		bool FillConstants( IEnumerable<ConstDeclaration> declarations )
		{
			foreach (var declaration in declarations)
			{
				var name = declaration.Name;
				if (ConstTable.ContainsKey( name ))
				{
					Error = "Const " + name + " declared twice";
					return false;
				}
				ConstTable.Add( name, declaration.Value );
			}
			return true;
		}

		uint AlignAddressToSize( uint address, uint size )
		{
			var mod = address % size;
			return mod == 0 ? address : address + (size - mod);
		}

		bool CalculateVarAddresses( IEnumerable<VarDeclaration> declarations )
		{
			foreach (var declaration in declarations)
			{
				var name = declaration.Name;
				if (VarTable.ContainsKey( name ))
				{
					Error = "Var " + name + " declared twice";
					return false;
				}
				uint totalSize = declaration.VarSize;
				if (declaration.IsArray)
				{
					if (declaration.ArrayLength.IsLiteral)
					{
						totalSize *= (uint)declaration.ArrayLength.Literal;
					}
					else
					{
						if (ResolveIdentifier( declaration.ArrayLength.Identifier, !declaration.ArrayLength.PositiveIdentifier, true, false, null, out long length ))
						{
							if (length < 1 || !LiteralFitsInSize( length, 2 ))
							{
								Error = "Bad array size";
								return false;
							}
							totalSize *= (uint)length;
						}
						else
						{
							Error = "Unknown constant";
							return false;
						}
					}
				}
				NextDataAddress = AlignAddressToSize( NextDataAddress, declaration.VarSize );
				VarTable.Add( name, new VarWithBaseAddress( declaration, NextDataAddress ) );
				NextDataAddress += totalSize;
			}
			return true;
		}

		bool EncodeCommands( IEnumerable<CommandNode> nodes )
		{
			foreach (var node in nodes)
			{
				if (node.IsLabel)
				{
					if (!AddLabel( node.LabelName, NextCodeAddress, true ))
					{
						return false;
					}
					continue;
				}
				var commandLength = EncodeSingleCommand( node, NextCodeAddress );
				if (commandLength == 0)
				{
					return false;
				}
				NextCodeAddress += commandLength;
			}
			return true;
		}

		uint EncodeSingleCommand( CommandNode node, uint address = 0 )
		{
			if (address == 0)
			{
				address = NextCodeAddress;
			}
			if (!_CheckErrors())
			{
				return 0;
			}
			Command command = node.ActualCommand;
			int index = Command.GetCommandIndex( command );
			byte firstByte = (byte)((index << 2) + (int)node.SizeMode);
			if (command.HasNoOperands())
			{
				SaveBytes( firstByte, 0, 0, 0 );
				return 4;
			}
			byte secondByte = (byte)((int)node.OperandsMode << 6);
			bool saveReg1 = command.HasTwoOperands() || (command.HasOneOperand() && node.OperandsMode == OperandsMode.RegReg);
			if (saveReg1)
			{
				var operand = command.HasTwoOperands() && node.OperandsMode == OperandsMode.MemReg ? node.Op2 : node.Op1;
				if (operand.IsRegister)
				{
					secondByte += (byte)(operand.RegisterIndex << 3);
				}
				else
				{
					Error = node.ToString() + " expected register as operand";
					return 0;
				}
				if (command.HasOneOperand())
				{
					SaveBytes( firstByte, secondByte, 0, 0 );
					return 4;
				}
			}
			bool saveReg2 = command.HasTwoOperands() && node.OperandsMode == OperandsMode.RegReg;
			if (saveReg2)
			{
				if (node.Op2.IsRegister)
				{
					secondByte += (byte)node.Op2.RegisterIndex;
					SaveBytes( firstByte, secondByte, 0, 0 );
					return 4;
				}
				else
				{
					Error = node.ToString() + " expected register as second operator";
					return 0;
				}
			}
			else if (node.OperandsMode == OperandsMode.RegImm)
			{
				var operand = command.HasTwoOperands() ? node.Op2 : node.Op1;
				if (!operand.IsImmediate)
				{
					Error = "Expected immediate";
					return 0;
				}
				long literal;
				if (operand.Immediate.IsLiteral)
				{
					literal = operand.Immediate.Literal;
				}
				else
				{
					var id = operand.Immediate.Identifier;
					if (!ResolveIdentifier( id, !operand.Immediate.PositiveIdentifier, true,
						true, operand.Immediate.IdentifierArrayIndex, out literal ))
					{
						literal = ((long)FindLabelAddress( id, node, address ) - address) / 4;
					}
				}
				if (node.SizeMode == SizeMode.FourBytes && command.GetCode() == CPU.MovInstructionCode &&
					!LiteralFitsInSize( literal, 2 ))
				{
					secondByte += 4;
					SaveBytes( firstByte, secondByte, 0, 0, true, (uint)(int)literal );
					return 8;
				}
				else
				{
					if (!LiteralFitsInSize( literal, 2 ))
					{
						Error = "Too big value";
						return 0;
					}
					SaveBytes( firstByte, secondByte, (byte)literal, (byte)(literal >> 8) );
					return 4;
				}
			}
			else
			{
				//memory
				var operand = command.HasOneOperand() || node.OperandsMode == OperandsMode.MemReg ? node.Op1 : node.Op2;
				if (!operand.IsAddress)
				{
					Error = "address exptected";
					return 0;
				}
				long scale;
				if (operand.AddressNode.Scale != null)
				{
					if (!GetLiteralOrConst( operand.AddressNode.Scale, out scale ))
					{
						Error = "Unknown const";
						return 0;
					}
					if (scale < 1 || scale > 128 || (scale & (scale - 1)) != 0)
					{
						Error = "Wrong scale";
						return 0;
					}
				}
				else
				{
					scale = 1;
				}
				uint scalePower = 0;
				while (scale != 1)
				{
					scale /= 2;
					scalePower++;
				}
				secondByte += (byte)scalePower;
				long constAddr;
				if (operand.AddressNode.Const != null)
				{
					if (!GetLiteralOrConst( operand.AddressNode.Const, out constAddr ))
					{
						Error = "Unknown const";
						return 0;
					}
					if (constAddr < -128 || constAddr > 127)
					{
						Error = "Wrong displacement";
						return 0;
					}
				}
				else
				{
					constAddr = 0;
				}
				byte lastByte = (byte)constAddr;
				byte thirdByte = 0;
				if (operand.AddressNode.FirstReg != null)
				{
					thirdByte = (byte)(operand.AddressNode.FirstReg.Value << 4);
				}
				else
				{
					thirdByte = 128;
				}
				if (operand.AddressNode.SecondReg != null)
				{
					thirdByte += (byte)operand.AddressNode.SecondReg.Value;
				}
				else
				{
					thirdByte += 8;
				}
				SaveBytes( firstByte, secondByte, thirdByte, lastByte );
				return 4;
			}

			bool _CheckErrors()
			{
				if (node.SizeMode == null)
				{
					Error = node.ToString() + " didn't had size mode";
					return false;
				}
				if (node.OperandsMode == null)
				{
					Error = node.ToString() + " didn't had operands mode";
					return false;
				}
				if (!node.ActualCommand.HasNoOperands() && node.Op1 == null)
				{
					Error = node.ToString() + " had no first operand";
					return false;
				}
				if (node.ActualCommand.HasTwoOperands() && node.Op2 == null)
				{
					Error = node.ToString() + " had no second operand";
					return false;
				}
				return true;
			}

			void SaveBytes( byte b3, byte b2, byte b1, byte b0, bool has4ByteImmediate = false, uint immediate4Bytes = 0 )
			{
				MemoryContent[address] = b3;
				MemoryContent[address + 1] = b2;
				MemoryContent[address + 2] = b1;
				MemoryContent[address + 3] = b0;
				if (has4ByteImmediate)
				{
					fixed (byte* ptr = &MemoryContent[address + 4])
					{
						*(uint*)ptr = immediate4Bytes;
					}
				}
			}
		}

		bool FillLiteral( long literal, uint address, uint size )
		{
			if (LiteralFitsInSize( literal, size ))
			{
				ulong unsigned = (ulong)literal;
				MemoryContent[address] = (byte)unsigned;
				if (size > 1)
				{
					MemoryContent[address + 1] = (byte)(unsigned >> 8);
				}
				if (size == 4)
				{
					MemoryContent[address + 2] = (byte)(unsigned >> 16);
					MemoryContent[address + 3] = (byte)(unsigned >> 24);
				}
				return true;
			}
			else
			{
				Error = "Value doesn't fit in var";
				return false;
			}
		}

		bool FillVarValues()
		{
			foreach (var variable in VarTable)
			{
				uint address = variable.Value.baseAddress;
				var declaration = variable.Value.declaration;
				if (declaration.IsArray)
				{
					if (declaration.ArrayOfValues == null)
					{
						continue;
					}
					foreach (var value in declaration.ArrayOfValues)
					{
						if (!_FillVar( address, value, declaration.VarSize ))
						{
							return false;
						}
						address += declaration.VarSize;
					}
				}
				else
				{
					if (declaration.SingleValue == null)
					{
						continue;
					}
					return _FillVar( address, declaration.SingleValue, declaration.VarSize );
				}
			}
			return true;

			bool _FillVar( uint address, SimpleValueNode value, uint size )
			{
				if (value.IsLiteral)
				{
					return _FillLiteral( value.Literal );
				}
				else
				{
					if (!ResolveIdentifier( value.Identifier, !value.PositiveIdentifier, true, false, null, out long literal ))
					{
						return false;
					}
					return _FillLiteral( literal );
				}

				bool _FillLiteral( long literal )
				{
					return FillLiteral( literal, address, size );
				}
			}
		}

		bool ResolveIdentifier( string name, bool returnNegative, bool searchConst, bool searchVars, SimpleValueNode? arrayIndex, out long value )
		{
			if (searchConst && ResolveConstant( name, out value ))
			{
				if (returnNegative)
				{
					value = -value;
				}
				return true;
			}
			if (searchVars && ResolveVarAddress( name, out value ))
			{
				if (returnNegative)
				{
					Error = "Trying to get negative address of var";
					return false;
				}
				if (arrayIndex != null)
				{
					var decl = VarTable[name].declaration;
					if (decl.IsArray)
					{
						long index;
						if (arrayIndex.IsLiteral)
						{
							index = arrayIndex.Literal;
						}
						else
						{
							if (!ResolveConstant( arrayIndex.Identifier, out index ))
							{
								Error = "Unknown constant";
								return false;
							}
						}
						value += index * decl.VarSize;
					}
					else
					{
						Error = "Trying to get index in non-array var";
						return false;
					}
				}
				return true;
			}
			value = 0;
			return false;
		}

		bool LiteralFitsInSize( long literal, uint size )
		{
			return (size == 2 && literal <= ushort.MaxValue && literal > short.MinValue)
				|| (size == 1 && literal <= byte.MaxValue && literal > sbyte.MinValue)
				|| (size == 4 && literal <= uint.MaxValue && literal > int.MinValue);
		}

		bool GetLiteralOrConst( SimpleValueNode node, out long value )
		{
			if (node.IsLiteral)
			{
				value = node.Literal;
				return true;
			}
			if (ConstTable.TryGetValue( node.Identifier, out value ))
			{
				return true;
			}
			value = 0;
			return false;
		}

		bool ResolveConstant( string name, out long value )
		{
			return ConstTable.TryGetValue( name, out value );
		}

		bool ResolveVarAddress( string name, out long address )
		{
			if (VarTable.TryGetValue( name, out VarWithBaseAddress output ))
			{
				address = output.baseAddress;
				return true;
			}
			address = 0;
			return false;
		}

		uint FindLabelAddress( string name, CommandNode node, uint commandAddress )
		{
			if (LabelsTable.TryGetValue( name, out var label ))
			{
				return label;
			}
			else
			{
				UnresolvedJumps.Add( new PendingLabel { commandStartAddress = commandAddress, labelName = name, node = node } );
				return commandAddress;
			}
		}

		bool AddLabel( string name, uint address, bool resolvePendingLabels )
		{
			if (LabelsTable.ContainsKey( name ))
			{
				Error = "Label " + name + " already exists";
				return false;
			}
			LabelsTable.Add( name, address );
			if (!resolvePendingLabels)
			{
				return true;
			}
			for (int i = UnresolvedJumps.Count - 1; i >= 0; i--)
			{
				var unresolved = UnresolvedJumps[i];
				if (unresolved.labelName == name)
				{
					EncodeSingleCommand( unresolved.node, unresolved.commandStartAddress );
					UnresolvedJumps.RemoveAt( i );
				}
			}
			return true;
		}

		struct VarWithBaseAddress
		{
			public VarDeclaration declaration;
			public uint baseAddress;

			public VarWithBaseAddress( VarDeclaration declaration, uint baseAddress )
			{
				this.declaration = declaration;
				this.baseAddress = baseAddress;
			}
		}

		struct PendingLabel
		{
			public CommandNode node;
			public uint commandStartAddress;
			public string labelName;
		}
	}
}
