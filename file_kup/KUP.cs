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

		[XmlIgnore]
		public uint RamOffsetUInt { get; set; }

		[XmlAttribute("ramOffset")]
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
				XmlReaderSettings xmlSettings = new XmlReaderSettings();
				xmlSettings.CheckCharacters = false;

				using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					KUP kup = (KUP)new XmlSerializer(typeof(KUP)).Deserialize(XmlReader.Create(fs, xmlSettings));
					kup.Resequence();
					return kup;
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
				xmlSettings.CheckCharacters = false;

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

		public void Resequence()
		{
			for (int i = 0; i < PointerTables.Count; i++)
				PointerTables[i].Sequence = (i + 1);
			for (int i = 0; i < StringBounds.Count; i++)
				StringBounds[i].Sequence = (i + 1);
		}
	}

	public class Entry : IEntry
	{
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

		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlIgnore]
		public long OffsetLong { get; set; }

		[XmlAttribute("offset")]
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

		[XmlIgnore]
		public byte[] OriginalText { get; set; }

		[XmlElement("original")]
		public string OriginalTextString
		{
			get { return Encoding.GetString(OriginalText); }
			set { OriginalText = Encoding.GetBytes(value); }
		}

		[XmlIgnore]
		public byte[] EditedText { get; set; }

		[XmlElement("edited")]
		public string EditedTextString
		{
			get { return Encoding.GetString(EditedText); }
			set { EditedText = Encoding.GetBytes(value); }
		}

		public bool IsResizable
		{
			get { return MaxLength == 0; }
		}

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

		public void AddPointer(long address)
		{
			Pointer pointer = null;
			foreach (Pointer p in Pointers)
				if (p.AddressLong == address)
				{
					pointer = p;
					break;
				}
			if (pointer == null)
				Pointers.Add(new Pointer(address));
		}

		public void PointerCleanup()
		{
			List<Pointer> pointers = new List<Pointer>();

			foreach (Pointer p in Pointers)
			{
				if (!pointers.Contains(p))
					pointers.Add(p);
			}

			Pointers.Clear();
			Pointers.AddRange(pointers);
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
		[XmlIgnore]
		public long StartLong { get; set; }

		[XmlAttribute("start")]
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

		[XmlIgnore]
		public long EndLong { get; set; }

		[XmlAttribute("end")]
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

		[XmlAttribute("sequence")]
		public int Sequence { get; set; }

		public Bound()
		{
			StartLong = 0;
			EndLong = 0;
			NextAvailableOffset = 0;
			Notes = string.Empty;
			Dumpable = false;
			Injectable = false;
			Sequence = 0;
		}

		public long SpaceRemaining
		{
			get { return EndLong - NextAvailableOffset; }
		}

		public bool Full
		{
			get { return SpaceRemaining <= 0; }
		}

		public override string ToString()
		{
			return "From " + Start + " to " + End;
		}
	}

	public class Pointer : IEquatable<Pointer>
	{
		[XmlIgnore]
		public long AddressLong { get; set; }

		[XmlAttribute("address")]
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

		public bool Equals(Pointer rhs)
		{
			return this.AddressLong == rhs.AddressLong;
		}
	}
}