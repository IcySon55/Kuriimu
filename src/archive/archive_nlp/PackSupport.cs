using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract;
using System.IO;
using Kontract.Compression;
using System;
using Kontract.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace archive_nlp.PACK
{
    public class PACKFileInfo : ArchiveFileInfo
    {
        public FileEntry Entry;
        public byte[] names;
        public byte[] pointers;

        public override Stream FileData
        {
            get
            {
                if (State != ArchiveFileState.Archived || Entry.entry.compSize == 0 || Entry.entry.compSize == Entry.entry.decompSize)
                    if (names != null && pointers != null)
                        return new SERI(base.FileData, names, pointers).data;
                    else
                        return base.FileData;
                return new MemoryStream(ZLib.Decompress(base.FileData));
            }
        }

        public override long? FileSize => Entry.entry.decompSize;

        public Tuple<uint, uint> Write(Stream input, uint compOffset, uint decompOffset)
        {
            using (var bw = new BinaryWriterX(input, true))
            {
                if (base.FileData.Length > 0)
                {
                    if (Entry.entry.compOffset != 0) Entry.entry.compOffset = compOffset;
                    Entry.entry.decompOffset = decompOffset;
                }

                if (State == ArchiveFileState.Archived)
                {
                    base.FileData.CopyTo(bw.BaseStream);
                    bw.WriteAlignment();
                }
                else
                {
                    if (base.FileData.Length > 0)
                    {
                        byte[] comp;
                        if (Entry.entry.compSize != 0 && Entry.entry.compSize != Entry.entry.decompSize)
                            comp = ZLib.Compress(base.FileData);
                        else
                            comp = new BinaryReaderX(base.FileData, true).ReadAllBytes();
                        bw.Write(comp);
                        bw.WriteAlignment();

                        if (Entry.entry.compSize != 0) Entry.entry.compSize = (uint)comp.Length;
                        Entry.entry.decompSize = (uint)base.FileData.Length;
                    }
                }

                return new Tuple<uint, uint>(
                    (Entry.entry.compOffset == 0) ?
                        Entry.entry.decompOffset + Entry.entry.decompSize :
                        Entry.entry.compOffset + Entry.entry.compSize,
                    decompOffset + Entry.entry.decompSize);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PACKHeader
    {
        public Magic magic;
        public ushort unk1;
        public ushort packFileCount;
        public uint stringOffOffset;
        public uint stringOffset;
        public uint fileOffset;
        public uint decompSize;
        public uint compSize;
        public uint zero1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PACKEntry
    {
        public Magic magic;
        public uint zero1;
        public uint decompSize;
        public uint decompOffset;
        public uint zero2;
        public uint unk1;
        public uint compSize;
        public uint compOffset;
    }

    public class FileEntry
    {
        public PACKEntry entry;
        public int nameOffset;
    }

    public class SERI
    {
        public Stream data = new MemoryStream();

        #region Deserilization
        public SERI(Stream fileData, byte[] names, byte[] pointers)
        {
            var output = new SERIList();

            using (var br = new BinaryReaderX(fileData, true))
            using (var namesB = new BinaryReaderX(new MemoryStream(names)))
            {
                var magic = br.ReadString(4);
                var valuesOffset = br.ReadInt32() + 4;
                var valuesCount = br.ReadInt16();
                var typeTableOffset = valuesOffset - valuesCount;

                for (int i = 0; i < valuesCount; i++)
                {
                    var nameOffset = br.ReadInt16();

                    namesB.BaseStream.Position = nameOffset;
                    var name = namesB.ReadCStringA();

                    var valueOffset = br.ReadInt16();

                    var bk = br.BaseStream.Position;
                    br.BaseStream.Position = typeTableOffset + i;
                    var type = br.ReadString(1);
                    br.BaseStream.Position = bk;

                    output.Add(ParseSERIParameter(
                        br.BaseStream,
                        namesB.BaseStream,
                        pointers,
                        type,
                        name,
                        valueOffset,
                        valuesOffset));
                }

                XmlSerializer Serializer = new XmlSerializer(typeof(SERIList));
                Serializer.Serialize(data, output);
            }
        }

        #region SERIList
        [XmlRoot]
        public class SERIList
        {
            [XmlArrayItem("Parameter")]
            public List<SERIParameter> Parameters = new List<SERIParameter>();

            public void Add(SERIParameter Parameter)
            {
                Parameters.Add(Parameter);
            }
        }
        #endregion

        #region SERIParameter
        [XmlInclude(typeof(String))]
        [XmlInclude(typeof(Integer))]
        [XmlInclude(typeof(Boolean))]
        [XmlInclude(typeof(Float))]
        [XmlInclude(typeof(StringArray))]
        [XmlInclude(typeof(IntegerArray))]
        [XmlInclude(typeof(BooleanArray))]
        [XmlInclude(typeof(FloatArray))]
        [XmlInclude(typeof(NestedArray))]
        public class SERIParameter
        {
            [XmlAttribute]
            public string Name;
        }

        public class String : SERIParameter
        {
            public string Value;

            public String(string Name, string Value)
            {
                this.Name = Name;
                this.Value = Value;
            }

            public String()
            {
            }
        }

        public class Integer : SERIParameter
        {
            public int Value;

            public Integer(string Name, int Value)
            {
                this.Name = Name;
                this.Value = Value;
            }

            public Integer()
            {
            }
        }

        public class Boolean : SERIParameter
        {
            public bool Value;

            public Boolean(string Name, bool Value)
            {
                this.Name = Name;
                this.Value = Value;
            }

            public Boolean()
            {
            }
        }

        public class Float : SERIParameter
        {
            public float Value;

            public Float(string Name, float Value)
            {
                this.Name = Name;
                this.Value = Value;
            }

            public Float()
            {
            }
        }

        public class StringArray : SERIParameter
        {
            [XmlArrayItem("Value")]
            public string[] Values;

            public StringArray(string Name, string[] Values)
            {
                this.Name = Name;
                this.Values = Values;
            }

            public StringArray()
            {
            }
        }

        public class IntegerArray : SERIParameter
        {
            [XmlArrayItem("Value")]
            public int[] Values;

            public IntegerArray(string Name, int[] Values)
            {
                this.Name = Name;
                this.Values = Values;
            }

            public IntegerArray()
            {
            }
        }

        public class BooleanArray : SERIParameter
        {
            [XmlArrayItem("Value")]
            public bool[] Values;

            public BooleanArray(string Name, bool[] Values)
            {
                this.Name = Name;
                this.Values = Values;
            }

            public BooleanArray()
            {
            }
        }

        public class FloatArray : SERIParameter
        {
            [XmlArrayItem("Value")]
            public float[] Values;

            public FloatArray(string Name, float[] Values)
            {
                this.Name = Name;
                this.Values = Values;
            }

            public FloatArray()
            {
            }
        }

        public class NestedArray : SERIParameter
        {
            [XmlArrayItem("Parameter")]
            public SERIParameter[] Values;

            public NestedArray(string Name, SERIParameter[] Values)
            {
                this.Name = Name;
                this.Values = Values;
            }

            public NestedArray()
            {
            }
        }
        #endregion

        public SERIParameter ParseSERIParameter(Stream fileData, Stream names, byte[] pointers, string type, string name, int valueOffset, int valuesOffset)
        {
            switch (type)
            {
                case "s": //String
                    names.Position = valueOffset;
                    using (var namesB = new BinaryReaderX(names, true))
                        return new String(name, namesB.ReadCStringA());
                case "i": //Integer
                    using (var br = new BinaryReaderX(fileData, true))
                    {
                        var bk = br.BaseStream.Position;
                        br.BaseStream.Position = valuesOffset + valueOffset;
                        int IntValue = br.ReadInt32();
                        br.BaseStream.Position = bk;

                        switch (name)
                        {
                            case "bone":
                            case "smes":
                            case "smat":
                            case "tex":
                            case "hair_length":
                                using (var pointersB = new BinaryReaderX(new MemoryStream(pointers)))
                                using (var namesB = new BinaryReaderX(names, true))
                                {
                                    pointersB.BaseStream.Position = (IntValue - 1) * 4;
                                    var tmp = pointersB.ReadInt32();

                                    namesB.BaseStream.Position = tmp;
                                    return new String(name, namesB.ReadCStringA());
                                }
                            default:
                                return new Integer(name, IntValue);
                        }
                    }
                case "b": //Boolean
                    using (var br = new BinaryReaderX(fileData, true))
                    {
                        var bk = br.BaseStream.Position;
                        br.BaseStream.Position = valuesOffset + valueOffset;
                        bool BooleanValue = br.ReadByte() == 1;
                        br.BaseStream.Position = bk;

                        return new Boolean(name, BooleanValue);
                    }
                case "f": //Float
                    using (var br = new BinaryReaderX(fileData, true))
                    {
                        var bk = br.BaseStream.Position;
                        br.BaseStream.Position = valuesOffset + valueOffset;
                        float FloatValue = br.ReadSingle();
                        br.BaseStream.Position = bk;

                        return new Float(name, FloatValue);
                    }
                case "a": //Array
                    throw new Exception($"Type {type} isn't supported!");
                /*using (var br = new BinaryReaderX(fileData, true))
                {
                    var bk = br.BaseStream.Position;
                    br.BaseStream.Position = valuesOffset + valueOffset;

                    var array = ParseSERIArray(
                        br.BaseStream,
                        names,
                        pointers,
                        valuesOffset,
                        name);

                    br.BaseStream.Position = bk;
                    return array;
                }*/
                default:
                    throw new Exception($"Type {type} is unknown!");
            }
        }

        public SERIParameter ParseSERIArray(Stream fileData, Stream names, byte[] pointers, int valuesOffset, string name)
        {
            using (var br = new BinaryReaderX(fileData, true))
            {
                var arrayDataType = br.ReadString(1);
                br.BaseStream.Position += 1;
                var arrayLength = br.ReadInt16();
                short[] pointersA = br.ReadMultiple<short>(arrayLength).ToArray();

                switch (arrayDataType)
                {
                    case "s":  //Of String
                        string[] StringArray = new string[arrayLength];

                        for (int i = 0; i < pointersA.Length; i++)
                        {
                            using (var namesB = new BinaryReaderX(names, true))
                            {
                                namesB.BaseStream.Position = pointersA[i];
                                string ItemName = namesB.ReadCStringA();
                                StringArray[i] = ItemName;
                            }
                        }

                        return new StringArray(name, StringArray);
                    case "i": //Of Integer
                        switch (name)
                        {
                            case "texi":
                            case "model":
                            case "cloth":
                            case "list":
                                string[] NameArray = new string[arrayLength];

                                for (int i = 0; i < pointersA.Length; i++)
                                {
                                    using (var namesB = new BinaryReaderX(names, true))
                                    using (var pointersB = new BinaryReaderX(new MemoryStream(pointers)))
                                    {
                                        br.BaseStream.Position = valuesOffset + pointersA[i];
                                        pointersB.BaseStream.Position = (br.ReadInt32() - 1) * 4;
                                        namesB.BaseStream.Position = pointersB.ReadInt32();
                                        string ItemName = namesB.ReadCStringA();
                                        NameArray[i] = ItemName;
                                    }
                                }

                                return new StringArray(name, NameArray);
                            default:
                                int[] IntArray = new int[arrayLength];

                                for (int i = 0; i < pointersA.Length; i++)
                                {
                                    br.BaseStream.Position = valuesOffset + pointersA[i];
                                    IntArray[i] = br.ReadInt32();
                                }

                                return new IntegerArray(name, IntArray);
                        }
                    case "b": //Of Boolean
                        bool[] BooleanArray = new bool[arrayLength];

                        for (int i = 0; i < pointersA.Length; i++)
                        {
                            br.BaseStream.Position = valuesOffset + pointersA[i];
                            BooleanArray[i] = br.ReadByte() == 1;
                        }

                        return new BooleanArray(name, BooleanArray);
                    case "f": //Of Float
                        float[] FloatArray = new float[arrayLength];

                        for (int i = 0; i < pointersA.Length; i++)
                        {
                            br.BaseStream.Position = valuesOffset + pointersA[i];
                            FloatArray[i] = br.ReadSingle();
                        }

                        return new FloatArray(name, FloatArray);
                    case "h": //Of Array
                        SERIParameter[] NestedArray = new SERIParameter[arrayLength];

                        for (int i = 0; i < pointersA.Length; i++)
                        {
                            br.BaseStream.Position = valuesOffset + pointersA[i];
                            ushort Count = br.ReadUInt16();
                            long TypesTableOffset = valuesOffset + pointersA[i] + 2 + Count * 4;
                            SERIParameter[] Arrays = new SERIParameter[Count];
                            for (int Index = 0; Index < Count; Index++)
                            {
                                using (var namesB = new BinaryReaderX(names, true))
                                {
                                    br.BaseStream.Position = valuesOffset + pointersA[i] + 2 + Index * 4;
                                    ushort NameOffset = br.ReadUInt16();
                                    ushort ValueOffset = br.ReadUInt16();

                                    namesB.BaseStream.Position = NameOffset;
                                    string ArrayName = namesB.ReadCStringA();

                                    br.BaseStream.Seek(TypesTableOffset + Index, SeekOrigin.Begin);
                                    string ValueType = br.ReadString(1);

                                    Arrays[Index] = ParseSERIParameter(
                                        br.BaseStream,
                                        names,
                                        pointers,
                                        ValueType,
                                        name,
                                        ValueOffset,
                                        valuesOffset);
                                }
                            }

                            NestedArray[i] = new NestedArray(null, Arrays);
                        }

                        return new NestedArray(name, NestedArray);
                    default:
                        throw new Exception($"Type {arrayDataType} is unknown!");
                }
            }
        }
        #endregion
    }
}
