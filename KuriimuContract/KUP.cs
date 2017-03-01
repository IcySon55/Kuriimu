using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace KuriimuContract
{
	[XmlRoot("kup")]
	public sealed class KUP
	{
		#region Properties

		[XmlIgnore]
		public Encoding Encoding { get; set; }

		[XmlAttribute("encoding")]
		public string EncodingString
		{
			get { return Encoding.WebName; }
			set
			{
				Encoding enc = Encoding.Unicode;
				try
				{
					enc = Encoding.GetEncoding(value);
				}
				catch (Exception) { }
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

		[XmlAttribute("optimizeStrings")]
		public bool OptimizeStrings { get; set; }

		[XmlArray("pointerTables")]
		[XmlArrayItem("pointerTable")]
		public List<Bound> PointerTables { get; set; }

		[XmlArray("stringBounds")]
		[XmlArrayItem("stringBound")]
		public List<Bound> StringBounds { get; set; }

		[XmlArray("entries")]
		[XmlArrayItem("entry")]
		public List<Entry> Entries { get; set; }

		[XmlIgnore]
		public int Count
		{
			get { return Entries.Count; }
		}

		#endregion

		public KUP()
		{
			Encoding = Encoding.Unicode;
			RamOffsetUInt = 0x0;
			OptimizeStrings = true;
			PointerTables = new List<Bound>();
			StringBounds = new List<Bound>();
			Entries = new List<Entry>();
		}

		public KUP(Encoding encoding) : this()
		{
			Encoding = encoding;
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
			catch (Exception)
			{
				throw;
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
			catch (Exception)
			{
				throw;
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

	public sealed class Entry : IEntry
	{
		// Interface
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

		[XmlIgnore]
		public long InjectedOffsetLong { get; set; }

		[XmlAttribute("relocatable")]
		public bool Relocatable { get; set; }

		[XmlElement("pointer")]
		public List<Pointer> Pointers { get; private set; }

		[XmlElement("original")]
		public string OriginalText { get; set; }

		[XmlElement("edited")]
		public string EditedText { get; set; }

		[XmlAttribute("max_length")]
		public int MaxLength { get; set; }

		[XmlIgnore]
		public IEntry ParentEntry { get; set; }

		[XmlIgnore]
		public bool IsSubEntry => ParentEntry != null;

		[XmlIgnore]
		public bool HasText { get; }

		[XmlIgnore]
		public List<IEntry> SubEntries { get; set; }

		[XmlArray("subEntries")]
		[XmlArrayItem("subEntry")]
		public List<Entry> SubEntriesXML
		{
			get { return SubEntries.ConvertAll(o => (Entry)o); }
			set { SubEntries = value.ConvertAll(o => (IEntry)o); }
		}

		public Entry()
		{
			Name = string.Empty;
			OffsetLong = 0x0;
			InjectedOffsetLong = 0x0;
			Relocatable = true;
			Pointers = new List<Pointer>();
			OriginalText = string.Empty;
			EditedText = string.Empty;
			MaxLength = 0;
			ParentEntry = null;
			HasText = true;
			SubEntries = new List<IEntry>();
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
			int result = Name.CompareTo(rhs.Name);
			if (result == 0)
				result = Offset.CompareTo(((Entry)rhs).Offset);
			return result;
		}
	}

	public sealed class Bound : IComparable<Bound>
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

		public long SpaceRemaining => EndLong - NextAvailableOffset;

		public bool Full => SpaceRemaining <= 0;

		public override string ToString()
		{
			return "From " + Start + " to " + End;
		}

		public int CompareTo(Bound rhs)
		{
			return StartLong.CompareTo(rhs.StartLong);
		}
	}

	public sealed class Pointer : IEquatable<Pointer>
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
			AddressLong = address;
		}

		public bool Equals(Pointer rhs)
		{
			return AddressLong == rhs.AddressLong;
		}
	}
}