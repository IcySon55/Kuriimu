using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace file_gmml
{
	[XmlRoot("gmml")]
	public sealed class GMML
	{
		#region Properties

		[XmlAttribute("version")]
		public string Version { get; set; }

		//[XmlElement("head")]
		//public Head Head { get; set; }

		[XmlElement("body")]
		public Body Body { get; set; }

		#endregion

		public GMML()
		{
			Version = "1.0";
			//Head = new Head();
			Body = new Body();
		}

		public static GMML Load(string filename)
		{
			try
			{
				XmlReaderSettings xmlSettings = new XmlReaderSettings();
				xmlSettings.CheckCharacters = false;

				using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					GMML gmm = (GMML)new XmlSerializer(typeof(GMML)).Deserialize(XmlReader.Create(fs, xmlSettings));
					return gmm;
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
				xmlSettings.Encoding = Encoding.Unicode;
				xmlSettings.Indent = true;
				xmlSettings.NewLineOnAttributes = false;
				xmlSettings.IndentChars = "	";
				xmlSettings.CheckCharacters = false;

				using (StreamWriter xmlIO = new StreamWriter(filename, false, xmlSettings.Encoding))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(GMML));
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

	//public class Head
	//{
	//	[XmlElement("create")]
	//	public Create Create { get; set; }

	//	[XmlElement("generator")]
	//	public Generator Generator { get; set; }

	//	public Head()
	//	{
	//		Create = new Create();
	//		Generator = new Generator();
	//	}
	//}

	//public class Create
	//{
	//	[XmlAttribute("user")]
	//	public string User { get; set; }

	//	[XmlAttribute("host")]
	//	public string Host { get; set; }

	//	[XmlIgnore]
	//	public DateTime Date { get; set; }

	//	[XmlAttribute("date")]
	//	public string DateString
	//	{
	//		get { return Date.ToString("yyyy-MM-ddTHH:mm:ss"); }
	//		set
	//		{
	//			DateTime date = DateTime.UtcNow;
	//			try
	//			{
	//				DateTime.TryParse(value, out date);
	//			}
	//			catch (Exception) { }
	//			Date = date;
	//		}
	//	}

	//	public Create()
	//	{
	//		User = string.Empty;
	//		Host = string.Empty;
	//		Date = DateTime.UtcNow;
	//	}
	//}

	//public class Generator
	//{
	//	[XmlAttribute("name")]
	//	public string Name { get; set; }

	//	[XmlAttribute("version")]
	//	public string Version { get; set; }

	//	public Generator()
	//	{
	//		Name = string.Empty;
	//		Version = string.Empty;
	//	}
	//}

	public sealed class Body
	{
		[XmlAttribute("language")]
		public string Language { get; set; }

		//[XmlElement("tag-table")]
		//public TagTable TagTable { get; set; }

		//[XmlElement("columns")]
		//public Columns Columns { get; set; }

		//[XmlElement("dialog")]

		//[XmlElement("output")]

		//[XmlElement("lock")]

		[XmlElement("row")]
		public List<Row> Rows { get; set; }

		//[XmlElement("flowchart-group-list")]

		public Body()
		{
			Language = string.Empty;
			//TagTable = new TagTable();
			Rows = new List<Row>();
		}
	}

	//public class TagTable
	//{
	//	[XmlElement("group")]
	//	public List<TagTableGroup> Groups { get; set; }

	//	public TagTable()
	//	{
	//		Groups = new List<TagTableGroup>();
	//	}
	//}

	//public class TagTableGroup
	//{
	//	[XmlAttribute("entry")]
	//	public int Entry { get; set; }

	//	[XmlAttribute("japanese")]
	//	public string Japanese { get; set; }

	//	[XmlAttribute("english")]
	//	public string English { get; set; }
	//}

	//#region Columns

	//public class Columns
	//{
	//	[XmlElement("id")]
	//	public ColumnsDimension ID { get; set; }

	//	[XmlElement("group")]
	//	public ColumnsGroup Group { get; set; }

	//	[XmlElement("number")]
	//	public ColumnsDimension Number { get; set; }

	//	[XmlElement("comment")]
	//	public ColumnsDimension Comment { get; set; }

	//	[XmlElement("erase")]
	//	public ColumnsDimension Erase { get; set; }

	//	[XmlElement("language")]
	//	public List<ColumnsLanguage> Languages { get; set; }

	//	public Columns()
	//	{
	//		ID = new ColumnsDimension();
	//		Group = new ColumnsGroup();
	//		Number = new ColumnsDimension();
	//		Comment = new ColumnsDimension();
	//		Erase = new ColumnsDimension();
	//		Languages = new List<ColumnsLanguage>();
	//	}
	//}

	//public class ColumnsDimension
	//{
	//	[XmlAttribute("default")]
	//	public string Default { get; set; }

	//	[XmlAttribute("position")]
	//	public int Position { get; set; }

	//	[XmlAttribute("width")]
	//	public int Width { get; set; }

	//	public ColumnsDimension()
	//	{
	//		Default = null;
	//		Position = -1;
	//		Width = 100;
	//	}
	//}

	//public class ColumnsGroup
	//{
	//	[XmlAttribute("default")]
	//	public string Default { get; set; }

	//	[XmlAttribute("position")]
	//	public int Position { get; set; }

	//	[XmlAttribute("width")]
	//	public int Width { get; set; }

	//	[XmlElement("list")]
	//	public List<ColumnsGroupList> Lists { get; set; }

	//	public ColumnsGroup()
	//	{
	//		Lists = new List<ColumnsGroupList>();
	//	}
	//}

	//public class ColumnsGroupList
	//{
	//	[XmlAttribute("entry")]
	//	public int Entry { get; set; }

	//	[XmlAttribute("japanese")]
	//	public string Japanese { get; set; }

	//	[XmlAttribute("english")]
	//	public string English { get; set; }

	//	[XmlAttribute("colorID")]
	//	public int ColorID { get; set; }

	//	[XmlAttribute("back")]
	//	public string Back { get; set; }
	//}

	//public class ColumnsLanguage
	//{
	//	[XmlAttribute("japanese")]
	//	public string Japanese { get; set; }

	//	[XmlAttribute("english")]
	//	public string English { get; set; }

	//	[XmlElement("message")]
	//	public ColumnsDimension Message { get; set; }

	//	[XmlElement("font")]
	//	public ColumnsDimension Font { get; set; }

	//	[XmlElement("size")]
	//	public ColumnsDimension Size { get; set; }

	//	[XmlElement("space")]
	//	public ColumnsDimension Space { get; set; }

	//	[XmlElement("width")]
	//	public ColumnsDimension Width { get; set; }

	//	[XmlElement("line")]
	//	public ColumnsDimension Line { get; set; }

	//	public ColumnsLanguage()
	//	{
	//		Message = new ColumnsDimension();
	//		Font = new ColumnsDimension();
	//		Size = new ColumnsDimension();
	//		Space = new ColumnsDimension();
	//		Width = new ColumnsDimension();
	//		Line = new ColumnsDimension();
	//	}
	//}

	//#endregion

	#region Rows

	public sealed class Row
	{
		[XmlAttribute("id")]
		public string ID { get; set; }

		[XmlElement("comment")]
		public string Comment { get; set; }

		[XmlElement("language")]
		public List<Language> Languages { get; set; }

		public Row()
		{
			ID = "0000";
			Comment = string.Empty;
			Languages = new List<Language>();
		}

		// Serialization Control
		public bool ShouldSerializeComment()
		{
			return Comment.Length > 0;
		}
	}

	public sealed class Language
	{
		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlText]
		public string Text { get; set; }

		public Language()
		{
			Name = string.Empty;
			Text = string.Empty;
		}
	}

	#endregion
}