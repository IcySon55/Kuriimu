using System;
using System.Text;

namespace file_jmsg
{
	public enum EncodingByte : byte
	{
		UTF8 = 0x00,
		Unicode = 0x01
	}

	public sealed class Header
	{
		public string Identifier; // jMSG
		public uint FileSize;
		public uint NumberOfEntries;
		public uint Version;
		public uint HasLabels;
		public uint LabelsOffset;

		public const uint Size = 0x18;
	}

	public sealed class Label
	{
		public uint NameOffset { get; set; }
		public string Name { get; set; }

		public uint ExtraID { get; set; }
		public uint ExtraOffset { get; set; }
		public string Extra { get; set; }

		public uint TextID { get; set; }
		public uint TextOffset { get; set; }
		public byte[] Text { get; set; }

		public Label()
		{
			NameOffset = 0;
			Name = string.Empty;

			ExtraID = 0;
			ExtraOffset = 0;
			Extra = string.Empty;

			TextID = 0;
			TextOffset = 0;
			Text = new byte[] { };
		}
	}

	public sealed class InvalidJMSGException : Exception
	{
		public InvalidJMSGException(string message) : base(message) { }
	}
}