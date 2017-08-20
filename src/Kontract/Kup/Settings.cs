using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Kuriimu.Kontract
{
    [XmlRoot("settings")]
    public sealed class KuriimuSettings
    {
        #region Properties

        [XmlArray("labels")]
        [XmlArrayItem("label")]
        public List<Label> Labels { get; set; }

        #endregion

        public KuriimuSettings()
        {
            Labels = new List<Label>();
        }

        public static KuriimuSettings Load(string filename)
        {
            XmlReaderSettings xmlSettings = new XmlReaderSettings();
            xmlSettings.CheckCharacters = false;

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                return (KuriimuSettings)new XmlSerializer(typeof(KuriimuSettings)).Deserialize(XmlReader.Create(fs, xmlSettings));
        }

        public bool Save(string filename)
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
                    XmlSerializer serializer = new XmlSerializer(typeof(KuriimuSettings));
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

    public sealed class Label
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("color")]
        public string Color { get; set; }

        public Label() { }

        public Label(string name, string color)
        {
            Name = name;
            Color = color;
        }
    }
}