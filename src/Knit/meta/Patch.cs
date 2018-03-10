using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Knit.steps;

namespace Knit
{
    [XmlRoot("patch")]
    public sealed class Patch
    {
        #region Properties

        /// <summary>
        /// The list of steps stored in this patch document.
        /// </summary>
        [XmlArray("steps")]
        [XmlArrayItem("step-test", typeof(StepTest))]
        [XmlArrayItem("step-select-file", typeof(StepSelectFile))]
        public List<Step> Steps { get; set; }

        #endregion

        public Patch()
        {
            Steps = new List<Step>();
        }

        /// <summary>
        /// This allows you to load a patch.xml document from disk.
        /// </summary>
        /// <param name="filename">The patch.xml to load.</param>
        /// <returns></returns>
        public static Patch Load(string filename)
        {
            try
            {
                using (var fs = File.OpenRead(filename))
                    return (Patch)new XmlSerializer(typeof(Patch)).Deserialize(XmlReader.Create(fs, new XmlReaderSettings { CheckCharacters = false }));
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
                    var serializer = new XmlSerializer(typeof(Patch));
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