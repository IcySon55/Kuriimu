using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using KuriimuContract;

namespace file_kup
{
	[XmlRoot("kup")]
	public class KUP
	{
		[XmlAttribute("count")]
		public int Count { get; set; }

		[XmlIgnore]
		public Encoding Encoding { get; set; }

		[XmlAttribute("encoding")]
		public string EncodingString
		{
			get { return Encoding.EncodingName; }
			set
			{
				Encoding enc = Encoding.Unicode;
				Encoding.GetEncoding(value);
				Encoding = enc;
			}
		}

		[XmlAttribute("ramOffset")]
		public uint RamOffsetUInt { get; set; }

		[XmlIgnore]
		public string RamOffset
		{
			get { return RamOffsetUInt.ToString("X2"); }
			set
			{
				uint val = 0;
				uint.TryParse(value, NumberStyles.HexNumber, null, out val);
				RamOffsetUInt = val;
			}
		}

		[XmlAttribute("file")]
		public string File { get; set; }

		[XmlArray("pointerTables")]
		[XmlArrayItem("pointerTable")]
		public List<Bound> PointerTables { get; set; }

		[XmlArray("stringBounds")]
		[XmlArrayItem("stringBound")]
		public List<Bound> StringBounds { get; set; }

		[XmlArray("entries")]
		[XmlArrayItem("entry")]
		public List<Entry> Entries { get; set; }

		public KUP()
		{
			this.Count = 0;
			this.Encoding = Encoding.Unicode;
			this.RamOffsetUInt = 0x0;
			this.PointerTables = new List<Bound>();
			this.StringBounds = new List<Bound>();
			this.Entries = new List<Entry>();
		}

		public KUP(Encoding encoding) : this()
		{
			this.Encoding = encoding;
		}

		public static KUP Load(string filename)
		{
			try
			{
				using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					return (KUP)new XmlSerializer(typeof(KUP)).Deserialize(fs);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public void Save(string filename)
		{
			try
			{
				XmlWriterSettings xmlSettings = new XmlWriterSettings();
				xmlSettings.Encoding = Encoding.UTF8;
				xmlSettings.Indent = true;
				xmlSettings.NewLineOnAttributes = false;
				xmlSettings.IndentChars = "	";

				using (StreamWriter xmlIO = new StreamWriter(filename, false, xmlSettings.Encoding))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(KUP));
					XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
					namespaces.Add(string.Empty, string.Empty);
					serializer.Serialize(XmlWriter.Create(xmlIO, xmlSettings), this, namespaces);
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}

	public class Entry : IEntry
	{
		[XmlIgnore]
		public Encoding Encoding { get; set; }

		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlAttribute("offset")]
		public long OffsetLong { get; set; }

		[XmlIgnore]
		public string Offset
		{
			get { return OffsetLong.ToString("X2"); }
			set
			{
				long val = 0;
				long.TryParse(value, NumberStyles.HexNumber, null, out val);
				OffsetLong = val;
			}
		}

		[XmlAttribute("relocatable")]
		public bool Relocatable { get; set; }

		[XmlAttribute("max_length")]
		public int MaxLength { get; set; }

		[XmlElement("pointer")]
		public List<Pointer> Pointers { get; private set; }

		[XmlElement("original")]
		public byte[] OriginalText { get; set; }

		[XmlElement("edited")]
		public byte[] EditedText { get; set; }

		public Entry()
		{
			this.Encoding = Encoding.Unicode;
			this.Name = string.Empty;
			this.OffsetLong = 0x0;
			this.Relocatable = true;
			this.MaxLength = 0;
			this.Pointers = new List<Pointer>();
			this.OriginalText = new byte[] { 0x0 };
			this.EditedText = new byte[] { 0x0 };
		}

		public Entry(Encoding encoding) : this()
		{
			this.Encoding = encoding;
		}

		public override string ToString()
		{
			return Name == string.Empty ? Offset : Name;
		}

		public string GetOriginalString()
		{
			return Encoding.GetString(OriginalText);
		}

		public string GetEditedString()
		{
			return Encoding.GetString(EditedText);
		}

		public int CompareTo(IEntry rhs)
		{
			int result = this.Name.CompareTo(rhs.Name);
			if (result == 0)
				result = this.Offset.CompareTo(((Entry)rhs).Offset);
			return result;
		}
	}

	public class Bound
	{
		[XmlAttribute("start")]
		public long StartLong { get; set; }

		[XmlIgnore]
		public string Start
		{
			get { return StartLong.ToString("X2"); }
			set
			{
				long val = 0;
				long.TryParse(value, NumberStyles.HexNumber, null, out val);
				StartLong = val;
			}
		}

		[XmlAttribute("end")]
		public long EndLong { get; set; }

		[XmlIgnore]
		public string End
		{
			get { return EndLong.ToString("X2"); }
			set
			{
				long val = 0;
				long.TryParse(value, NumberStyles.HexNumber, null, out val);
				EndLong = val;
			}
		}

		[XmlIgnore]
		public long NextAvailableOffset { get; set; }

		[XmlAttribute("notes")]
		public string Notes { get; set; }

		[XmlAttribute("dumpable")]
		public bool Dumpable { get; set; }

		[XmlAttribute("injectable")]
		public bool Injectable { get; set; }

		public Bound()
		{
			StartLong = 0;
			EndLong = 0;
			NextAvailableOffset = 0;
			Notes = string.Empty;
			Dumpable = false;
			Injectable = false;
		}

		public long SpaceRemaining
		{
			get { return EndLong - NextAvailableOffset; }
		}

		public bool Full
		{
			get { return SpaceRemaining == 0; }
		}

		public override string ToString()
		{
			return "From " + Start + " to " + End;
		}
	}

	public class Pointer
	{
		[XmlAttribute("address")]
		public long AddressLong { get; set; }

		[XmlIgnore]
		public string Address
		{
			get { return AddressLong.ToString("X2"); }
			set
			{
				long val = 0;
				long.TryParse(value, NumberStyles.HexNumber, null, out val);
				AddressLong = val;
			}
		}

		public Pointer() { }

		public Pointer(long address)
		{
			this.AddressLong = address;
		}
	}
}