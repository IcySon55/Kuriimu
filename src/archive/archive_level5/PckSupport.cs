using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kontract.Interface;
using System.Xml.Serialization;

namespace archive_level5.PCK
{
    public class PckFileInfo : ArchiveFileInfo
    {
        public Entry Entry;
        public List<uint> Hashes;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint hash;
        public int fileOffset;
        public int fileLength;
    }

    public class DictXmlClass
    {
        [XmlElement("Dicts")]
        public List<Dict> dict;
    }
    public class Dict
    {
        [XmlAttribute]
        public string pckName;
        [XmlElement("keyValues")]
        public List<Item<uint, string>> keyValuePairs;
    }
    public class Item<T, T2>
    {
        public T key;
        public T2 value;
    }
}
