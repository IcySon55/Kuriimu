using System;
using System.Collections.Generic;
using System.IO;
using Kuriimu.Kontract;
using Kuriimu.IO;
using System.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace text_gmm
{
    public sealed class GMM
    {
        public List<Label> Labels = new List<Label>();

        Header header;
        List<GMMTextEntry> entries = new List<GMMTextEntry>();

        public GMM(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename), true))
            {
                //Header
                header = br.ReadStruct<Header>();

                //TextEntries
                for (int i = 0; i < header.entryCount; i++)
                {
                    entries.Add(new GMMTextEntry(br.BaseStream));
                }

                //Texts
                var textDataSize = br.ReadUInt16();
                for (int i = 0; i < header.entryCount; i++)
                {
                    br.BaseStream.Position = 4 + header.entryCount * 0xc + entries[i].textDataOffset;
                    var size = br.ReadUInt16();
                    Labels.Add(new Label
                    {
                        Text = Encoding.Unicode.GetString(br.ReadBytes(size)),
                        TextID = i,
                        Name = entries[i].name
                    });
                }
            }
        }

        public void Save(string filename)
        {
            using (var bw = new BinaryWriterX(File.Create(filename)))
            {
                //To change
                //Header - fileSize
                bw.BaseStream.Position = 4;

                //TextEntries
                uint textDataOffset = 2;
                for (int i = 0; i < Labels.Count; i++)
                {
                    bw.WriteASCII(Labels[i].Name);
                    for (int j = 8 - Labels[i].Name.Length; j > 0; j--) bw.Write((byte)0);
                    bw.Write(textDataOffset);
                    textDataOffset += (uint)Encoding.Unicode.GetBytes(Labels[i].Text).Length + 2;
                }

                //Texts
                bw.BaseStream.Position += 2;
                for (int i = 0; i < Labels.Count; i++)
                {
                    var text = Encoding.Unicode.GetBytes(Labels[i].Text);
                    bw.Write((ushort)text.Length);
                    bw.Write(text);
                }
                bw.BaseStream.Position = 4 + Labels.Count * 0xc;
                bw.Write((ushort)(bw.BaseStream.Length - (4 + Labels.Count * 0xc)));

                //Header
                bw.BaseStream.Position = 0;
                header.fileSize = (ushort)bw.BaseStream.Length;
                bw.WriteStruct(header);
            }
        }
    }
}
