using System.Collections.Generic;
using System.IO;
using Kontract;
using Kontract.IO;
using Kontract.Compression;
using System.Text;
using System.Linq;

namespace text_papa
{
    public sealed class PAPA
    {
        public List<Label> Labels = new List<Label>();
        private string _version = "";
        public PAPA(string filename, string version)
        {
            _version = version;
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename)))
            {
                var magic = br.ReadStruct<Magic8>();

                var offsetHeader = br.ReadStruct<OffsetHeader>();
                var offsets = br.ReadMultiple<int>(offsetHeader.entryCount);

                var textID = 0;
                foreach (var offset in offsets)
                {
                    var text = "";
                    var name = "";
                    br.BaseStream.Position = offset;
                    var entryHeader = br.ReadStruct<EntryHeader>();
                    if (entryHeader.dataSize == 0 && entryHeader.metaCount == 0)
                        break;
                    var meta = br.ReadMultiple<int>(entryHeader.metaCount);
                    // JPN version
                    if (version == "JPN")
                    {
                        br.BaseStream.Position = offset + meta[1];
                        text = br.ReadCStringW();

                        br.BaseStream.Position = offset + meta[2];
                        name = br.ReadCStringA();
                    }
                    // USA version
                    else
                    {
                        br.BaseStream.Position = offset + meta[1];
                        name = br.ReadCStringA();

                        br.BaseStream.Position = offset + meta[2];
                        text = br.ReadCStringW();
                    }
                    Labels.Add(new Label
                    {
                        TextID = textID++,
                        Text = text,
                        Name = name
                    });
                }
            }
        }

        public void Save(string filename)
        {
            var ascii = Encoding.ASCII;
            var utf16 = Encoding.Unicode;

            var texts = new List<byte[]>();
            var names = new List<byte[]>();
            foreach (var label in Labels)
            {
                texts.Add(utf16.GetBytes(label.Text + "\0"));
                names.Add(ascii.GetBytes(label.Name + "\0"));
            }

            var papaHeaderSize = 0x8;
            var entryHeaderSize = 0x14;
            var entryCount = Labels.Count;

            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                //PAPA Header
                bw.WriteASCII("PAPA\0\0\0\0");

                //Offset list
                var offsetHeader = new OffsetHeader();
                offsetHeader.entryCount = entryCount + 1;
                offsetHeader.dataSize = (entryCount + 1) * sizeof(int) + papaHeaderSize;
                bw.WriteStruct(offsetHeader);

                //Offsets
                var initOff = offsetHeader.headerSize + offsetHeader.dataSize;
                for (int i = 0; i < entryCount; i++)
                {
                    bw.Write(initOff);
                    // Idk this codes actually mean, but both methods are works, I just keep original code.
                    //if (_version == "JPN")
                    //{
                        initOff += entryHeaderSize + 4 + texts[i].Length;
                        initOff = (initOff + 3) & ~3;
                        initOff += names[i].Length;
                        initOff = (initOff + 3) & ~3;
                    //}
                    /*
                    else
                    {
                        initOff += entryHeaderSize + 4 + names[i].Length;
                        initOff = (initOff + 3) & ~3;
                        initOff += texts[i].Length;
                        initOff = (initOff + 3) & ~3;
                    }
                    */
                    
                }
                bw.Write(initOff);

                //Entries
                for (int i = 0; i < entryCount; i++)
                {
                    var entryDataSize = (((entryHeaderSize + 4 + texts[i].Length + 3) & ~3) + names[i].Length + 3) & ~3;

                    bw.Write(entryDataSize);
                    bw.Write(3);
                    bw.Write(0x14);
                    bw.Write(0x18);
                    if (_version == "JPN")
                        bw.Write((entryHeaderSize + 4 + texts[i].Length + 3) & ~3);
                    else
                        bw.Write((entryHeaderSize + 4 + names[i].Length + 3) & ~3);

                    bw.WriteASCII("Msg\0");
                    // JPN version
                    if (_version == "JPN")
                    {
                        bw.Write(texts[i]);
                        bw.WriteAlignment(4);
                        bw.Write(names[i]);
                        bw.WriteAlignment(4);
                    }
                    else
                    {
                        bw.Write(names[i]);
                        bw.WriteAlignment(4);
                        bw.Write(texts[i]);
                        bw.WriteAlignment(4);
                    }
                    
                }

                //End Mark Entry
                bw.Write(0);
                bw.Write(0);
            }
        }
    }
}
