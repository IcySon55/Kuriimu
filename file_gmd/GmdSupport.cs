using System;

namespace file_gmd
{
	public sealed class Header
	{
		public string Identifier; // GMD
		public byte[] Unknown1; // 8 Bytes Version?
		public uint Unknown2;
		public byte[] Unknown3; // 4 Bytes
		public uint NumberOfLabels;
		public uint NumberOfOffsets;
		public byte[] Unknown4; // 4 Bytes
		public uint DataSize;
		public uint NameLength;
		public string Name;
	}

	public sealed class Position
	{
		public uint ID;
		public uint Offset;
	}

	public sealed class Label
	{
		public uint ID;
		public uint Unknown1;
		public uint Unknown2;
		public uint Unknown3;
		public uint Unknown4;
		public string Name;

		public byte[] Text = new byte[] { };
	}

	public sealed class InvalidGMDException : Exception
	{
		public InvalidGMDException(string message) : base(message) { }
	}
}