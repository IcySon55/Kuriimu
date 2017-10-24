using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Kontract.Interface;

namespace Kontract
{
    [XmlRoot("lot")]
    public sealed class Lot
    {
        #region Properties

        [XmlArray("lotEntries")]
        [XmlArrayItem("lotEntry")]
        public List<LotEntry> LotEntries { get; set; }

        #endregion

        public Lot()
        {
            LotEntries = new List<LotEntry>();
        }

        public void Populate(List<TextEntry> entries)
        {
            foreach (var entry in entries)
            {
                var lotEntry = new LotEntry { Entry = entry.Name };
                LotEntries.Add(lotEntry);

                foreach (var subEntry in entry.SubEntries)
                {
                    var lotSubEntry = new LotEntry { Entry = subEntry.Name };
                    lotEntry.LotSubEntries.Add(lotSubEntry);
                }
            }
        }

        public static Lot Load(string filename)
        {
            XmlReaderSettings xmlSettings = new XmlReaderSettings();
            xmlSettings.CheckCharacters = false;

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                return (Lot)new XmlSerializer(typeof(Lot)).Deserialize(XmlReader.Create(fs, xmlSettings));
        }

        public bool Save(string filename)
        {
            try
            {
                XmlWriterSettings xmlSettings = new XmlWriterSettings();
                xmlSettings.Encoding = Encoding.UTF8;
                xmlSettings.Indent = true;
                xmlSettings.NewLineOnAttributes = false;
                xmlSettings.NewLineHandling = NewLineHandling.Entitize;
                xmlSettings.IndentChars = "	";
                xmlSettings.CheckCharacters = false;

                using (StreamWriter xmlIO = new StreamWriter(filename, false, xmlSettings.Encoding))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Lot));
                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    serializer.Serialize(XmlWriter.Create(xmlIO, xmlSettings), this, namespaces);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }

    public sealed class LotEntry
    {
        [XmlAttribute("entry")]
        public string Entry { get; set; }

        [XmlAttribute("labelID")]
        public string LabelID { get; set; }

        [XmlElement("notes")]
        public string Notes { get; set; }

        [XmlElement("screenshot")]
        public List<Screenshot> Screenshots { get; set; }

        [XmlArray("lotSubEntries")]
        [XmlArrayItem("lotSubEntry")]
        public List<LotEntry> LotSubEntries { get; set; }

        public LotEntry()
        {
            Entry = string.Empty;
            LabelID = string.Empty;
            Notes = string.Empty;
            Screenshots = new List<Screenshot>();
            LotSubEntries = new List<LotEntry>();
        }
    }

    public sealed class Screenshot
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlText]
        public string BitmapBase64 { get; set; }

        [XmlIgnore]
        public Bitmap Bitmap
        {
            get
            {
                var bytes = Convert.FromBase64String(BitmapBase64);
                using (var ms = new MemoryStream())
                {
                    ms.Write(bytes, 0, bytes.Length);
                    return new Bitmap(ms);
                }
            }
            set
            {
                using (var ms = new MemoryStream())
                {
                    value.Save(ms, ImageFormat.Png);
                    var bytes = ms.ToArray();
                    BitmapBase64 = Convert.ToBase64String(bytes);
                }
            }
        }

        public Screenshot()
        {
            Name = string.Empty;
            BitmapBase64 = string.Empty;
        }
    }
}