using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Kuriimu.Kontract
{
    [XmlRoot("kupUser")]
    public sealed class KupUser
    {
        #region Properties

        [XmlElement("filename")]
        public string Filename { get; set; }

        #endregion

        public KupUser()
        {
            Filename = string.Empty;
        }

        public static KupUser Load(string filename)
        {
            KupUser user = null;

            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    user = (KupUser)new XmlSerializer(typeof(KupUser)).Deserialize(fs);
                }
            }
            catch (Exception)
            {
                user = new KupUser();
            }

            return user;
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
                    XmlSerializer serializer = new XmlSerializer(typeof(KupUser));
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
}