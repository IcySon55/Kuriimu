using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Kuriimu
{
	class Shift_JIS
	{
		public void Process(string[] args)
		{
			FileStream arm9 = File.Open(args[0], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			BinaryReader arm9r = new BinaryReader(arm9, Encoding.ASCII);

			FileStream output = File.Open(args[1], FileMode.Create, FileAccess.Write, FileShare.Read);

			byte b = 0x0;
			byte p = 0x0A;
			long position = 0;
			long length = 0;

			long start = 0xB1CE0;
			long end = 0xB9DF0;

			arm9.Seek(start, SeekOrigin.Begin);

			while (arm9.Position < end)
			{
				position = arm9r.BaseStream.Position;
				b = (byte)arm9.ReadByte();

				bool isDouble = false;
				byte peek = (byte)arm9.ReadByte();
				arm9.Seek(-1, SeekOrigin.Current);

				if (IsValid(b, peek, out isDouble))
				{
					length = 0;
					List<byte> bytes = new List<byte>();
					if (isDouble)
					{
						bytes.Add(b);
						b = (byte)arm9.ReadByte();
						bytes.Add(b);
						length += 2;
					}
					else
					{
						bytes.Add(b);
						length++;
					}
					bool valid = true;

					while (arm9r.PeekChar() != 0x0 && arm9r.PeekChar() != -1)
					{
						b = (byte)arm9.ReadByte();
						peek = (byte)arm9.ReadByte();
						arm9.Seek(-1, SeekOrigin.Current);

						if (IsValid(b, peek, out isDouble))
						{
							if (isDouble)
							{
								bytes.Add(b);
								b = (byte)arm9.ReadByte();
								bytes.Add(b);
								length += 2;
							}
							else
							{
								bytes.Add(b);
								length++;
							}
						}
						else
						{
							valid = false;
							break;
						}
					}

					while (valid && arm9r.PeekChar() != -1)
					{
						b = (byte)arm9.ReadByte();

						if (b == 0x0)
						{
							length++;
						}
						else
						{
							arm9.Seek(-1, SeekOrigin.Current);

							bool containsShift = false;

							for (int i = 0; i < bytes.Count - 1; i++)
							{
								bool subShift = false;
								IsValid(bytes[i], bytes[i + 1], out subShift);

								if (subShift)
									containsShift = true;
							}

							if (bytes.ToArray<byte>().Length > 1 && bytes.ToArray<byte>()[0] != 0x0A && containsShift)
							{
								byte[] bits;

								//MySqlConnection connection = new MySqlConnection("Server=localhost;Port=3307;Uid=root;Pwd=k4gami17;Database=coropata;");
								//connection.Open();

								bits = new byte[8];
								Encoding.ASCII.GetBytes("String:\n", 0, 8, bits, 0);
								output.Write(bits, 0, 8);

								output.Write(bytes.ToArray<byte>(), 0, bytes.ToArray<byte>().Length);

								bits = new byte[11 + position.ToString("X").Length];
								Encoding.ASCII.GetBytes("\nPosition: " + position.ToString("X"), 0, 11 + position.ToString("X").Length, bits, 0);
								output.Write(bits, 0, 11 + position.ToString("X").Length);

								bits = new byte[10 + length.ToString().Length];
								Encoding.ASCII.GetBytes("\nLength: " + length + "\n", 0, 10 + length.ToString().Length, bits, 0);
								output.Write(bits, 0, 10 + length.ToString().Length);

								output.Write(new byte[16] { 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x3D, 0x0A }, 0, 16);

								// Commented out the MySql stuff as I've successfully imported the strings already.
								//MySqlCommand cmd = new MySqlCommand("import_saveTranslation", connection);

								//cmd.CommandType = CommandType.StoredProcedure;
								//cmd.Parameters.AddWithValue("inImportID", 0);
								//cmd.Parameters.AddWithValue("inJP_Text", Encoding.GetEncoding(932).GetString(bytes.ToArray<byte>()));
								//cmd.Parameters.AddWithValue("inByteOffset", position.ToString("X"));
								//cmd.Parameters.AddWithValue("inByteSize", length);

								//cmd.ExecuteNonQuery();
								//connection.Close();
							}

							break;
						}
					}
				}
			}
		}

		private static bool IsValid(byte b, byte p, out bool isDouble)
		{
			bool valid = false;
			isDouble = false;

			if (
				(
				// Shift-JIS ranges
					(b == 0x81 && (p >= 0x3F && p <= 0xFF))
				) || (
					(b == 0x82 && (p >= 0x4F && p <= 0xFF))
				) || (
					(b == 0x83 && (p >= 0x3F && p <= 0xFF))
				) || (
					(b == 0x84 && (p >= 0x3F && p <= 0xBF))
				) || (
					(b == 0x87 && (p >= 0x3F && p <= 0x9F))
				) || (
					(b == 0x88 && (p >= 0x9E && p <= 0xFF))
				) || (
					((b >= 0x89 && b <= 0x9F) && (p >= 0x3F && p <= 0xFF)) // Large 1st byte range that all have the same 2nd byte range
				) || (
					((b >= 0xE0 && b <= 0xE9) && (p >= 0x3F && p <= 0xFF)) // Large 1st byte range that all have the same 2nd byte range
				) || (
					(b == 0xEA && (p >= 0x3F && p <= 0x9F))
				) || (
					((b >= 0xED && b <= 0xEE) && (p >= 0x40 && p <= 0xFF))
				) || (
					((b == 0xFA && b <= 0xFB) && (p >= 0x40 && p <= 0xFF))
				) || (
					(b == 0xFC && (p >= 0x40 && p <= 0x4F))
				)
			)
			{
				valid = true;
				isDouble = true;
			}
			else if (
				// Standard ASCII ranges
					(b == 0x09 || b == 0x0A || b == 0x0D || (b >= 0x20 && b <= 0x7E))
				)
			{
				valid = true;
				isDouble = false;
			}

			return valid;
		}
	}
}