using System;
using System.Collections.Generic;
using System.Text;

namespace file_btxt
{
	public sealed class Header
	{
		public byte[] Identifier; // 0x00 00 00 00 24 10 12 FF
		public ushort NumberOfLabels;
		public ushort NumberOfStrings;
	}

	public sealed class Label
	{
		public uint StringCount { get; set; }

		public string Name { get; set; }
		public List<String> Strings { get; set; }

		public Label()
		{
			Name = string.Empty;
			Strings = new List<String>();
		}
	}

	public sealed class String
	{
		public uint ID { get; set; }
		public string Text { get; set; }

		public String()
		{
			ID = 0;
			Text = string.Empty;
		}
	}

	public sealed class InvalidBTXTException : Exception
	{
		public InvalidBTXTException(string message) : base(message) { }
	}
}