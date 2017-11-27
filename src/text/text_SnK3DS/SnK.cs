using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Text;
using Kontract.IO;
using Kontract;

namespace text_SnK3DS
{
    public class SnK
    {
        public List<Label> Labels = new List<Label>();

        private List<byte[]> controls = new List<byte[]>();

        public SnK(string filename)
        {
            using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename)))
            {
                var bytes = br.ReadAllBytes().ToList();
                var entryCount = 1;
                while (bytes.Count() > 0)
                {
                    var startOffset = (int)br.BaseStream.Position;

                    var index = bytes.FindIndex(b => b == 0x04);

                    if (index != -1)
                    {
                        //check for false positives
                        br.BaseStream.Position = startOffset + index + 1;
                        var tmpSize = br.ReadInt16();
                        if (tmpSize >= br.BaseStream.Length && tmpSize + br.BaseStream.Position >= br.BaseStream.Length)
                        {
                            var indexList = bytes.Select((b, i) => (b == 0x04) ? i : -1).Where(b => b != -1).ToList();
                            var count = 1;
                            while ((tmpSize >= br.BaseStream.Length && tmpSize + br.BaseStream.Position >= br.BaseStream.Length) && count < indexList.Count())
                            {
                                br.BaseStream.Position = startOffset + indexList[count++] + 1;
                                tmpSize = br.ReadInt16();
                            }

                            if (count < indexList.Count)
                            {
                                index = indexList[count - 1];
                            }
                            else
                            {
                                controls.Add(bytes.ToArray());
                                break;
                            }
                        }

                        br.BaseStream.Position = startOffset;

                        controls.Add(br.ReadBytes(index));
                        br.BaseStream.Position++;

                        var stringSize = br.ReadInt16();
                        Labels.Add(new Label
                        {
                            Name = "Entry " + entryCount,
                            TextID = entryCount++,
                            Text = br.ReadCStringSJIS()
                        });

                        bytes.RemoveRange(0, (int)br.BaseStream.Position - startOffset);
                    }
                    else
                    {
                        controls.Add(bytes.ToArray());
                        break;
                    }
                }
            }
        }

        public void Save(string filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {
                for (int i = 0; i < Labels.Count(); i++)
                {
                    bw.Write(controls[i]);
                    bw.Write((byte)4);

                    var text = Encoding.GetEncoding("SJIS").GetBytes(Labels[i].Text);
                    bw.Write((short)(text.Count() + 1));
                    bw.Write(text);
                    bw.Write((byte)0);
                }
                if (controls.Count > Labels.Count())
                {
                    bw.Write(controls.Last());
                }
            }
        }
    }
}
