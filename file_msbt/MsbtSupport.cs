using System;
using System.Collections.Generic;

namespace file_msbt
{
	public enum EncodingByte : byte
	{
		UTF8 = 0x00,
		Unicode = 0x01
	}

	public sealed class Header
	{
		public string Identifier; // MsgStdBn
		public byte[] ByteOrderMark;
		public UInt16 Unknown1; // Always 0x0000
		public EncodingByte EncodingByte;
		public byte Unknown2; // Always 0x03
		public UInt16 NumberOfSections;
		public UInt16 Unknown3; // Always 0x0000
		public UInt32 FileSize;
		public byte[] Unknown4; // Always 0x0000 0000 0000 0000 0000

		public UInt32 FileSizeOffset;
	}

	public abstract class Section
	{
		public string Identifier;
		public UInt32 SectionSize; // Begins after Unknown1
		public byte[] Padding1; // Always 0x0000 0000
	}

	public sealed class LBL1 : Section
	{
		public UInt32 NumberOfGroups;

		public List<Group> Groups;
		public List<Label> Labels;
	}

	public sealed class Group
	{
		public UInt32 NumberOfLabels;
		public UInt32 Offset;
	}

	public sealed class Label
	{
		public UInt32 Length { get; set; }
		public string Name { get; set; }
		public UInt32 Index { get; set; }

		public UInt32 Checksum { get; set; }
		public String String { get; set; }

		public byte[] Text
		{
			get
			{
				return String.Text;
			}
			set
			{
				String.Text = value;
			}
		}

		public Label()
		{
			Length = 0;
			Name = string.Empty;
			Index = 0;
			Checksum = 0;
			String = new String();
		}
	}

	public sealed class NLI1 : Section
	{
		public byte[] Unknown2; // Tons of unknown data
	}

	public sealed class ATO1 : Section
	{
		public byte[] Unknown2; // Large collection of 0xFF
	}

	public sealed class ATR1 : Section
	{
		public UInt32 NumberOfAttributes;
		public byte[] Unknown2; // Tons of unknown data
	}

	public sealed class TSY1 : Section
	{
		public byte[] Unknown2; // Tons of unknown data
	}

	public sealed class TXT2 : Section
	{
		public UInt32 NumberOfStrings;

		public List<String> Strings;
	}

	public sealed class String
	{
		public byte[] Text { get; set; }

		public UInt32 Index { get; set; }

		public String()
		{
			Text = new byte[] { };
			Index = 0;
		}
	}

	public sealed class InvalidMSBTException : Exception
	{
		public InvalidMSBTException(string message) : base(message) { }
	}
}