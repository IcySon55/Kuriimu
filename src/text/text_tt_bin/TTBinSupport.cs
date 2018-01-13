using System.Collections.Generic;
using Kontract.Interface;
using System.Runtime.InteropServices;
using System.IO;
using Kontract.IO;
using System.Linq;

namespace tt.text_ttbin
{
    #region Entry_Definition
    public sealed class Entry : TextEntry
    {
        // Interface
        public string Name
        {
            get { return EditedLabel.Name; }
            set { }
        }

        public string OriginalText => OriginalLabel.Text;

        public string EditedText
        {
            get { return EditedLabel.Text; }
            set { EditedLabel.Text = value; }
        }

        public int MaxLength { get; set; }

        public TextEntry ParentEntry { get; set; }

        public bool IsSubEntry => ParentEntry != null;

        public bool HasText { get; }

        public List<TextEntry> SubEntries { get; set; }

        // Adapter
        public Label OriginalLabel { get; }
        public Label EditedLabel { get; set; }

        public Entry()
        {
            OriginalLabel = new Label();
            EditedLabel = new Label();

            Name = string.Empty;
            MaxLength = 0;
            ParentEntry = null;
            HasText = true;
            SubEntries = new List<TextEntry>();
        }

        public Entry(Label editedLabel) : this()
        {
            if (editedLabel != null)
                EditedLabel = editedLabel;
        }

        public Entry(Label editedLabel, Label originalLabel) : this(editedLabel)
        {
            if (originalLabel != null)
                OriginalLabel = originalLabel;
        }

        public override string ToString()
        {
            return Name == string.Empty ? EditedLabel.TextOffset.ToString("X2") : Name;
        }
    }
    #endregion

    #region Label_Definition
    public sealed class Label
    {
        public string Name;

        public uint TextID;
        public uint TextOffset;
        public string Text;

        public Label()
        {
            Name = string.Empty;

            TextID = 0;
            TextOffset = 0;
            Text = string.Empty;
        }
    }
    #endregion

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public uint entryCount;
        public uint dataOffset;
        public uint dataLength;
        public uint unk1; //maybe textsCount?
    }

    public class CfgEntry
    {
        public CfgEntry(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                hash = br.ReadUInt32();
                entryCount = br.ReadByte();
                var types = br.ReadBytes(GetBytesToRead(entryCount)).Reverse();

                var scannedTypes = 0;
                foreach (var typeChunk in types)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        var type = (byte)((typeChunk >> i * 2) & 0x3);
                        if (type != 0x3)
                        {
                            object value = null;
                            switch (type)
                            {
                                case 0:
                                    value = br.ReadInt32();
                                    break;
                                case 1:
                                    value = br.ReadInt32();
                                    break;
                                case 2:
                                    value = br.ReadSingle();
                                    break;
                            }

                            metaInfo.Add(new Meta
                            {
                                type = type,
                                value = value
                            });

                            scannedTypes++;
                        }

                        if (scannedTypes >= entryCount) break;
                    }

                    if (scannedTypes >= entryCount) break;
                }
            }
        }

        int GetBytesToRead(int entryCount)
        {
            if (entryCount <= 12)
                return 3;
            else
            {
                var longerEntry = entryCount - 12;
                var bytesToRead = longerEntry / 16 * 4 + 3;
                return (longerEntry % 16 == 0) ? bytesToRead : bytesToRead + 4;
            }
        }

        public void Write(Stream input)
        {
            //Create Types Bitfield
            var types = new List<byte>();

            int tmpCount = entryCount;
            while (tmpCount > 0)
            {
                byte typeChunk = 0;
                for (int i = 0; i < 4; i++)
                {
                    typeChunk <<= 2;
                    typeChunk |= metaInfo[tmpCount - 1].type;

                    if (--tmpCount <= 0)
                    {
                        break;
                    }
                }
                types.Add(typeChunk);
            }
            types.Reverse();
            while ((types.Count() + 1) % 4 != 0) types.Add(0xff);

            //Writing the entry
            using (var bw = new BinaryWriterX(input, true))
            {
                bw.Write(hash);
                bw.Write(entryCount);
                bw.Write(types.ToArray());
                foreach (var meta in metaInfo)
                    switch (meta.type)
                    {
                        case 0:
                            bw.Write((int)meta.value);
                            break;
                        case 1:
                            bw.Write((int)meta.value);
                            break;
                        case 2:
                            bw.Write((float)meta.value);
                            break;
                    }
            }
        }

        public uint hash;
        public byte entryCount;
        public List<Meta> metaInfo = new List<Meta>();

        public class Meta
        {
            public byte type;
            public object value;
        }
    }
}