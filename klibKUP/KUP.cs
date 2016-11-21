using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KuriimuContract;
using System.Xml;
using System.IO;

namespace klibKUP
{
	public class KUP : IFileAdapter
	{
		private FileInfo _targetFile = null;
		private List<IEntry> _entries = new List<IEntry>();
		private XmlDocument _xmlDocument = null;

		#region Properties

		// Information
		public string Name
		{
			get { return "KUP"; }
		}

		public string Description
		{
			get { return "Kuriimu Archive"; }
		}

		public string Extension
		{
			get { return "*.kup"; }
		}

		public string About
		{
			get { return "This is the KUP file adapter for Kuriimu."; }
		}

		// Feature Support
		public bool CanSave
		{
			get { return true; }
		}

		public bool CanAddEntries
		{
			get { return true; }
		}

		public bool CanRemoveEntries
		{
			get { return true; }
		}

		#endregion

		public FileInfo TargetFile
		{
			get
			{
				return _targetFile;
			}
			set
			{
				_targetFile = value;
			}
		}

		public LoadResult Load(string filename)
		{
			Encoding encoding = Encoding.Unicode;
			LoadResult result = LoadResult.Success;

			_targetFile = new FileInfo(filename);

			if (File.Exists(filename))
			{
				try
				{
					_xmlDocument = new XmlDocument();
					_xmlDocument.Load(_targetFile.FullName);

					XmlNode root = _xmlDocument.SelectSingleNode("/kuriimu");

					if (root.Attributes["encoding"] != null)
						encoding = Encoding.GetEncoding(root.Attributes["encoding"].Value);

					// Load Entries
					XmlNodeList xmlEntries = _xmlDocument.SelectNodes("/kuriimu/entries/entry");
					foreach (XmlNode xmlEntry in xmlEntries)
					{
						Entry entry = new Entry(encoding);

						entry.Offset = Convert.ToInt64(xmlEntry.Attributes["offset"].Value, 16);

						if (xmlEntry.Attributes["name"] != null)
							entry.Name = xmlEntry.Attributes["name"].Value;

						// Pointers
						if (xmlEntry.Attributes["pointer"] != null)
							entry.Pointers.Add(Convert.ToInt64(xmlEntry.Attributes["pointer"].Value, 16));

						foreach (XmlNode pointer in xmlEntry.SelectNodes("pointer"))
						{
							if (pointer.Attributes["address"] != null)
								entry.Pointers.Add(Convert.ToInt64(pointer.Attributes["address"].Value, 16));
						}

						bool matched = false;
						foreach (Entry ntr in _entries)
						{
							if (entry.Offset == ntr.Offset)
							{
								ntr.Pointers.AddRange(entry.Pointers);
								matched = true;
								break;
							}
						}
						if (matched)
							continue;

						// Text
						XmlNode xmlOriginal = xmlEntry.SelectSingleNode("text");
						if (xmlOriginal == null)
							xmlOriginal = xmlEntry.SelectSingleNode("original");
						if (xmlOriginal != null)
							entry.OriginalText = encoding.GetBytes(xmlOriginal.InnerText);

						XmlNode xmlEdited = xmlEntry.SelectSingleNode("translation");
						if (xmlEdited == null)
							xmlEdited = xmlEntry.SelectSingleNode("edited");
						if (xmlEdited != null)
							entry.EditedText = encoding.GetBytes(xmlEdited.InnerText);

						// Length
						if (xmlEntry.Attributes["max_length"] != null)
							entry.MaxLength = Convert.ToInt32(xmlEntry.Attributes["max_length"].Value);

						_entries.Add(entry);
					}
				}
				catch (XmlException)
				{
					result = LoadResult.TypeMismatch;
				}
				catch (Exception)
				{
					result = LoadResult.Failure;
				}
			}
			else
				result = LoadResult.FileNotFound;

			return result;
		}

		public SaveResult Save(string filename = "")
		{
			SaveResult result = SaveResult.Success;

			if (filename.Trim() != string.Empty)
				_targetFile = new FileInfo(filename);

			try
			{
				XmlWriterSettings xmlSettings = new XmlWriterSettings();
				xmlSettings.Encoding = Encoding.UTF8;
				xmlSettings.Indent = true;
				xmlSettings.IndentChars = "\t";
				xmlSettings.CheckCharacters = false;

				XmlNode xmlEntries = _xmlDocument.SelectSingleNode("/kuriimu/entries");
				xmlEntries.RemoveAll();

				// Save Entries
				foreach (Entry entry in _entries)
				{
					XmlElement xmlEntry = _xmlDocument.CreateElement("entry");
					xmlEntries.AppendChild(xmlEntry);

					XmlAttribute xmlAttribute = null;

					if (entry.Name.Trim() != string.Empty)
					{
						xmlAttribute = _xmlDocument.CreateAttribute("name");
						xmlAttribute.Value = entry.Name;
						xmlEntry.Attributes.Append(xmlAttribute);
					}

					xmlAttribute = _xmlDocument.CreateAttribute("offset");
					xmlAttribute.Value = entry.Offset.ToString("X2");
					xmlEntry.Attributes.Append(xmlAttribute);

					if (entry.MaxLength > 0)
					{
						xmlAttribute = _xmlDocument.CreateAttribute("max_length");
						xmlAttribute.Value = entry.MaxLength.ToString();
						xmlEntry.Attributes.Append(xmlAttribute);
					}

					// Pointers
					if (entry.Pointers.Count > 0)
					{
						foreach (long pointer in entry.Pointers)
						{
							XmlElement xmlPointer = _xmlDocument.CreateElement("pointer");
							xmlEntry.AppendChild(xmlPointer);

							xmlAttribute = _xmlDocument.CreateAttribute("address");
							xmlAttribute.Value = pointer.ToString("X2");
							xmlPointer.Attributes.Append(xmlAttribute);
						}
					}

					// Text
					XmlElement xmlString = _xmlDocument.CreateElement("original");
					xmlString.InnerText = entry.GetOriginalString();
					xmlEntry.AppendChild(xmlString);

					xmlString = _xmlDocument.CreateElement("edited");
					xmlString.InnerText = entry.GetEditedString();
					xmlEntry.AppendChild(xmlString);
				}

				System.IO.StreamWriter stream = new StreamWriter(_targetFile.FullName, false, Encoding.UTF8);
				_xmlDocument.Save(XmlWriter.Create(stream, xmlSettings));
				stream.Close();
			}
			catch (Exception)
			{
				result = SaveResult.Failure;
			}

			return result;
		}

		// Entries
		public bool HasEntries
		{
			get { return _entries.Count > 0; }
		}

		public List<IEntry> Entries
		{
			get { return _entries; }
		}

		// Features
		public bool AddEntry(IEntry entry)
		{
			bool result = true;

			try
			{
				_entries.Add(entry);
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		public bool RemoveEntry(IEntry entry)
		{
			bool result = true;

			try
			{
				_entries.Remove(entry);
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}
	}

	public class Entry : IEntry
	{
		public Encoding Encoding { get; set; }
		public string Name { get; set; }
		public byte[] OriginalText { get; set; }
		public byte[] EditedText { get; set; }

		public long Offset { get; set; }
		public List<long> Pointers { get; private set; }
		public int MaxLength { get; set; }

		public bool HasPointers
		{
			get { return Pointers.Count > 0 && MaxLength == 0; }
		}

		public Entry(Encoding encoding)
		{
			this.Encoding = encoding;
			this.Name = string.Empty;
			this.OriginalText = new byte[] { 0x0 };
			this.EditedText = new byte[] { 0x0 };

			this.Offset = 0x0;
			this.Pointers = new List<long>();
			this.MaxLength = 0;
		}

		public override string ToString()
		{
			return Name == string.Empty ? Offset.ToString("X2") : Name;
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
}