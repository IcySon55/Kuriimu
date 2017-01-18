using System;
using System.Text;

namespace file_btxt
{
	public enum EncodingByte : byte
	{
		UTF8 = 0x00,
		Unicode = 0x01
	}

	public sealed class Header
	{
		public byte[] Identifier; // 0x00 00 00 00 24 10 12 FF
		public ushort NumberOfEntries;
		public ushort NumberOfUnknown1;
		public uint LabelLength;
	}

	public sealed class Object1
	{
		public uint Value1;
		public uint Value2;
	}

	public sealed class Label
	{
		public string Name { get; set; }
		public byte[] Text { get; set; }

		public Label()
		{
			Name = string.Empty;
			Text = new byte[] { };
		}
	}

	public sealed class InvalidBTXTException : Exception
	{
		public InvalidBTXTException(string message) : base(message) { }
	}
}