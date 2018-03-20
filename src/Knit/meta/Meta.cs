using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Knit
{
    [XmlRoot("meta")]
    public sealed class Meta
    {
        #region Properties

        [XmlElement("title")]
        public string Title { get; set; } = "Knit";

        [XmlElement("version")]
        public string Version { get; set; } = string.Empty;

        [XmlElement("author")]
        public string Author { get; set; } = string.Empty;

        [XmlElement("icon")]
        public string Icon { get; set; } = "icon.ico";

        [XmlElement("color")]
        public string Color { get; set; } = "#000000";

        [XmlElement("background")]
        public string Background { get; set; } = "background.png";

        [XmlElement("button")]
        public string Button { get; set; } = "Patch...";

        #endregion

        /// <summary>
        /// Initializes a new instance of the Meta class that is empty.
        /// </summary>
        public Meta() { }

        /// <summary>
        /// Loads a meta.xml document from disk.
        /// </summary>
        /// <param name="filename">The meta.xml to load.</param>
        /// <returns></returns>
        public static Meta Load(string filename)
        {
            try
            {
                using (var fs = File.OpenRead(filename))
                    return (Meta)new XmlSerializer(typeof(Meta)).Deserialize(XmlReader.Create(fs, new XmlReaderSettings { CheckCharacters = false }));
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Saves the meta.xml document to disk.
        /// </summary>
        /// <param name="filename">The filename to save to.</param>
        public void Save(string filename)
        {
            try
            {
                var xmlSettings = new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    Indent = true,
                    NewLineOnAttributes = false,
                    NewLineHandling = NewLineHandling.Entitize,
                    IndentChars = "	",
                    CheckCharacters = false
                };

                using (var xmlIO = new StreamWriter(filename, false, xmlSettings.Encoding))
                {
                    var serializer = new XmlSerializer(typeof(Meta));
                    var namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    serializer.Serialize(XmlWriter.Create(xmlIO, xmlSettings), this, namespaces);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}