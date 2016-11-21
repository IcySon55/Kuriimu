using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Security.Cryptography;
using KuriimuContract;

namespace Kuriimu
{
	class RocketSlime
	{
		private uint ram = 0x100000;

		public void DumpPointerTable(string[] args)
		{
			Encoding enc = Encoding.Unicode;
			uint startOffset = UInt32.Parse(args[1]);
			uint endOffset = UInt32.Parse(args[2]);
			FileInfo file = new FileInfo(args[3]);

			FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
			BinaryReaderX br = new BinaryReaderX(fs, ByteOrder.LittleEndian);
			br.BaseStream.Seek(startOffset, SeekOrigin.Begin);

			// Vars
			List<byte> result = new List<byte>();
			Dictionary<long, Entry> dict = new Dictionary<long, Entry>();
			long min = br.BaseStream.Length;
			long max = 0;

			// Do
			Console.WriteLine("Working...");

			Entry item = new Entry(enc);
			item.Offset = br.BaseStream.Position;

			while (br.BaseStream.Position < endOffset)
			{
				byte[] unichar = br.ReadBytes(2);

				if (unichar[0] != 0x0 || unichar[1] != 0x0)
					result.AddRange(unichar);
				else
				{
					item.Value = result.ToArray();
					result.Clear();
					dict.Add(item.Offset + ram, item);

					// Next Item
					item = new Entry(enc);
					item.Offset = br.BaseStream.Position;
				}
			}

			br.BaseStream.Seek(0, SeekOrigin.Begin);

			while (br.BaseStream.Position < br.BaseStream.Length - 4)
			{
				uint value = br.ReadUInt32();

				if (dict.ContainsKey(value))
				{
					dict[value].Pointer = br.BaseStream.Position - 4;

					min = Math.Min(min, dict[value].Pointer);
					max = Math.Max(max, dict[value].Pointer);
				}
			}

			Console.WriteLine("Done!");
			br.Close();

			XmlDocument xmlDocument = new XmlDocument();
			SHA512 shaM = new SHA512Managed();

			XmlWriterSettings xmlSettings = new XmlWriterSettings();
			xmlSettings.Encoding = enc;
			xmlSettings.Indent = true;
			xmlSettings.IndentChars = "\t";
			xmlSettings.CheckCharacters = false;

			XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", enc.WebName, null);
			xmlDocument.AppendChild(xmlDeclaration);

			XmlElement xmlRoot = xmlDocument.CreateElement("kuriimu");
			xmlDocument.AppendChild(xmlRoot);

			XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("table_min_offset");
			xmlAttribute.Value = min.ToString("X2");
			xmlRoot.Attributes.Append(xmlAttribute);

			xmlAttribute = xmlDocument.CreateAttribute("table_max_offset");
			xmlAttribute.Value = max.ToString("X2");
			xmlRoot.Attributes.Append(xmlAttribute);

			System.IO.StreamWriter stream = new StreamWriter(file.Name + ".table.xml", false, enc);
			xmlDocument.Save(XmlWriter.Create(stream, xmlSettings));
			stream.Close();
		}

		public void DumpStrings(string filename)
		{
			Encoding enc = Encoding.Unicode;
			FileInfo file = new FileInfo(filename);

			FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
			BinaryReaderX br = new BinaryReaderX(fs, ByteOrder.LittleEndian);

			List<uint> starts = new List<uint> { 0x3C07F8, 0x3C4A74, 0x3C55E8, 0x3093A0 };
			List<uint> ends = new List<uint> { 0x3C48D4, 0x3C4AA4, 0x3C59A0, 0x309EF4 };
			List<Entry> items = new List<Entry>();

			for (int i = 0; i < starts.Count; i++)
			{
				br.BaseStream.Seek(starts[i], SeekOrigin.Begin);

				// Vars
				List<byte> result = new List<byte>();
				List<uint> notNull = new List<uint>();

				notNull.Add(0x13 + 0x00);

				while (br.BaseStream.Position < ends[i])
				{
					uint offset = br.ReadUInt32() - ram;
					if (offset > br.BaseStream.Length - 3)
						continue;

					long saved = br.BaseStream.Position;

					br.BaseStream.Seek(offset, SeekOrigin.Begin);

					byte[] previous = new byte[] { 0x0, 0x0 };
					byte[] unichar = br.ReadBytes(2);
					while (unichar[0] + unichar[1] != 0 || (notNull.Contains((uint)previous[0] + previous[1]) && unichar[0] + unichar[1] == 0))
					{
						result.AddRange(unichar);
						unichar.CopyTo(previous, 0);
						unichar = br.ReadBytes(2);
					}

					Entry item = new Entry(enc);
					item.Offset = offset;
					item.Pointer = saved - 4;
					item.Value = result.ToArray();
					item.MaxLength = (br.BaseStream.Position - item.Offset) / 2 - 1;

					if (item.Value.Length > 0 && enc.GetString(item.Value).Trim() != string.Empty)
						items.Add(item);

					result.Clear();
					br.BaseStream.Seek(saved, SeekOrigin.Begin);
				}
			}

			br.Close();

			XmlDocument xmlDocument = new XmlDocument();

			XmlWriterSettings xmlSettings = new XmlWriterSettings();
			xmlSettings.Encoding = enc;
			xmlSettings.Indent = true;
			xmlSettings.IndentChars = "\t";
			xmlSettings.CheckCharacters = false;

			if (File.Exists(file.Name + ".xml"))
			{
				xmlDocument.Load(file.Name + ".xml");

				foreach (Entry i in items)
				{
					XmlNode xmlEntries = xmlDocument.SelectSingleNode("/kuriimu/entries");
					XmlNode node = xmlDocument.SelectSingleNode("//entry[@offset='" + i.Offset.ToString("X2") + "']");

					if (node != null)
					{
						XmlNode text = node.SelectSingleNode("text");
						text.InnerText = enc.GetString(i.Value);

						text = node.SelectSingleNode("translation");
						text.InnerText = enc.GetString(i.Value);
					}
					else
					{
						XmlElement xmlEntry = xmlDocument.CreateElement("entry");
						xmlEntries.AppendChild(xmlEntry);

						XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("offset");
						xmlAttribute.Value = i.Offset.ToString("X2");
						xmlEntry.Attributes.Append(xmlAttribute);

						xmlAttribute = xmlDocument.CreateAttribute("pointer");
						xmlAttribute.Value = i.Pointer.ToString("X2");
						xmlEntry.Attributes.Append(xmlAttribute);

						xmlAttribute = xmlDocument.CreateAttribute("length");
						xmlAttribute.Value = i.MaxLength.ToString();
						xmlEntry.Attributes.Append(xmlAttribute);

						XmlElement xmlString = xmlDocument.CreateElement("text");
						xmlString.InnerText = enc.GetString(i.Value);
						xmlEntry.AppendChild(xmlString);

						xmlString = xmlDocument.CreateElement("translation");
						xmlString.InnerText = enc.GetString(i.Value);
						xmlEntry.AppendChild(xmlString);
					}
				}

				System.IO.StreamWriter stream = new StreamWriter(file.Name + ".xml", false, enc);
				xmlDocument.Save(XmlWriter.Create(stream, xmlSettings));
				stream.Close();
			}
			else
			{
				XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", enc.WebName, null);
				xmlDocument.AppendChild(xmlDeclaration);

				XmlElement xmlRoot = xmlDocument.CreateElement("kuriimu");
				xmlDocument.AppendChild(xmlRoot);

				XmlAttribute xmlAttribute = xmlDocument.CreateAttribute("count");
				xmlAttribute.Value = items.Count.ToString();
				xmlRoot.Attributes.Append(xmlAttribute);

				XmlElement xmlEntries = xmlDocument.CreateElement("entries");
				xmlRoot.AppendChild(xmlEntries);

				foreach (Entry i in items)
				{
					XmlElement xmlEntry = xmlDocument.CreateElement("entry");
					xmlEntries.AppendChild(xmlEntry);

					xmlAttribute = xmlDocument.CreateAttribute("offset");
					xmlAttribute.Value = i.Offset.ToString("X2");
					xmlEntry.Attributes.Append(xmlAttribute);

					xmlAttribute = xmlDocument.CreateAttribute("pointer");
					xmlAttribute.Value = i.Pointer.ToString("X2");
					xmlEntry.Attributes.Append(xmlAttribute);

					xmlAttribute = xmlDocument.CreateAttribute("length");
					xmlAttribute.Value = i.MaxLength.ToString();
					xmlEntry.Attributes.Append(xmlAttribute);

					XmlElement xmlString = xmlDocument.CreateElement("text");
					xmlString.InnerText = enc.GetString(i.Value);
					xmlEntry.AppendChild(xmlString);

					xmlString = xmlDocument.CreateElement("translation");
					xmlString.InnerText = enc.GetString(i.Value);
					xmlEntry.AppendChild(xmlString);
				}

				System.IO.StreamWriter stream = new StreamWriter(file.Name + ".xml", false, enc);
				xmlDocument.Save(XmlWriter.Create(stream, xmlSettings));
				stream.Close();
			}
		}

		public void InjectStrings(string code_bin, string xml)
		{
			Encoding enc = Encoding.Unicode;
			File.Copy(code_bin, code_bin + ".edited", true);
			FileInfo codeFile = new FileInfo(code_bin + ".edited");
			FileInfo xmlFile = new FileInfo(xml);

			List<Bounds> bounds = new List<Bounds>();
			List<Entry> entries = new List<Entry>();
			List<Entry> injectedEntries = new List<Entry>();

			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(xmlFile.FullName);

			XmlNode Kuriimu = xmlDocument.SelectSingleNode("/kuriimu");
			enc = Encoding.GetEncoding(Kuriimu.Attributes["encoding"].Value);

			// Load Bounds
			XmlNodeList xmlBounds = xmlDocument.SelectNodes("/kuriimu/bounds/bound");
			foreach (XmlNode xmlBound in xmlBounds)
			{
				Bounds bound = new Bounds();
				bound.StartOffset = Convert.ToUInt32(xmlBound.Attributes["start_offset"].Value, 16);
				bound.EndOffset = Convert.ToUInt32(xmlBound.Attributes["end_offset"].Value, 16);
				bound.NextAvailableOffset = bound.StartOffset;
				bounds.Add(bound);
			}

			// Load Entries
			XmlNodeList xmlEntries = xmlDocument.SelectNodes("/kuriimu/entries/entry");
			foreach (XmlNode xmlEntry in xmlEntries)
			{
				Entry entry = new Entry(enc);
				entry.Offset = Convert.ToUInt32(xmlEntry.Attributes["offset"].Value, 16);
				if (xmlEntry.Attributes["pointer"] != null)
					entry.Pointer = Convert.ToUInt32(xmlEntry.Attributes["pointer"].Value, 16);
				entry.Value = enc.GetBytes(xmlEntry.SelectSingleNode("translation").InnerText);
				if (xmlEntry.Attributes["max_length"] != null)
					entry.MaxLength = Convert.ToUInt32(xmlEntry.Attributes["max_length"].Value);
				entries.Add(entry);
			}

			// Sort Entries
			entries.Sort();
			entries.Reverse();

			// Begin Injection
			FileStream fs = new FileStream(codeFile.FullName, FileMode.Open, FileAccess.Write, FileShare.Read);
			BinaryWriterX bw = new BinaryWriterX(fs, ByteOrder.LittleEndian);

			Console.WriteLine("Injection started.");

			bool outOfSpace = false;
			int count = 0;
			foreach (Entry entry in entries)
			{
				count++;

				if (entry.HasPointer)
				{
					// Optimization pass
					bool optimized = false;
					foreach (Entry i in injectedEntries)
					{
						if (i.ToString().EndsWith(entry.ToString()))
						{
							// Update the pointer
							bw.BaseStream.Seek(entry.Pointer, SeekOrigin.Begin);
							bw.Write((uint)(i.Offset + (i.Value.Length - entry.Value.Length) + ram));
							optimized = true;
							break;
						}
					}

					if (!optimized)
					{
						// Select bound
						Bounds bound = null;
						foreach (Bounds i in bounds)
						{
							if (!i.Full && entry.Value.Length < i.SpaceRemaining)
							{
								bound = i;
								break;
							}
						}

						if (bound != null)
						{
							// Update the pointer
							bw.BaseStream.Seek(entry.Pointer, SeekOrigin.Begin);
							bw.Write(bound.NextAvailableOffset + ram);
							entry.Offset = bound.NextAvailableOffset;

							// Write the string
							bw.BaseStream.Seek(bound.NextAvailableOffset, SeekOrigin.Begin);
							bw.Write(entry.Value);
							if (entry.Encoding.IsSingleByte)
								bw.Write(new byte[] { 0x0 });
							else
								bw.Write(new byte[] { 0x0, 0x0 });
							bound.NextAvailableOffset = (uint)bw.BaseStream.Position;

							injectedEntries.Add(entry);
						}
						else
						{
							// Ran out of injection space
							outOfSpace = true;
							break;
						}
					}
				}
				else
				{
					// In place string update
					bw.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
					bw.Write(entry.Value);
					if (entry.Encoding.IsSingleByte)
						bw.Write(new byte[] { 0x0 });
					else
						bw.Write(new byte[] { 0x0, 0x0 });
				}

				if (count % 100 == 0)
					Console.WriteLine(count);
			}

			if (outOfSpace)
			{
				Console.WriteLine("The injector has run out of space to inject the strings.");
				Console.WriteLine((entries.Count - count) + " strings were not injected.");
			}
			else
			{
				Console.WriteLine(count);
				Console.WriteLine("Injection completed.");
			}

			bw.Close();
		}
	}

	class Entry : IComparable<Entry>
	{
		public Encoding Encoding { get; set; }
		public long Offset { get; set; }
		public long Pointer { get; set; }
		public byte[] Value { get; set; }
		public long MaxLength { get; set; }

		public bool HasPointer
		{
			get { return Pointer > 0 && MaxLength == 0; }
		}

		public Entry(Encoding encoding)
		{
			this.Encoding = encoding;
			this.Offset = 0;
			this.Pointer = 0;
			this.Value = null;
			this.MaxLength = 0;
		}

		public override string ToString()
		{
			return Encoding.GetString(Value);
		}

		public int CompareTo(Entry rhs)
		{
			int result = this.Value.Length.CompareTo(rhs.Value.Length);
			if (result == 0)
			{
				string myString = this.Encoding.GetString(this.Value);
				string theirString = this.Encoding.GetString(rhs.Value);
				result= myString.CompareTo(theirString);
			}
			return result;
		}
	}

	public class Bounds
	{
		public uint StartOffset { get; set; }
		public uint EndOffset { get; set; }
		public uint NextAvailableOffset { get; set; }

		public uint SpaceRemaining
		{
			get { return EndOffset - NextAvailableOffset; }
		}

		public bool Full
		{
			get { return SpaceRemaining == 0; }
		}

		public Bounds()
		{
			StartOffset = 0;
			EndOffset = 0;
			NextAvailableOffset = 0;
		}
	}
}