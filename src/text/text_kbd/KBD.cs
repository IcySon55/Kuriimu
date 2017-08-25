using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kuriimu.IO;

namespace text_kbd
{
    public sealed class KBD
    {
        public List<Label> Labels = new List<Label>();
        public List<ushort> textIdent = new List<ushort> { 0x38, 0x72 };
        public ushort minLimit = 0x600;    //this value isn't based on anything, it was chosen randomly after researching the file values, it can be adjusted if needed
        public ushort maxLimit = 0xfe00;    //this value isn't based on anything, it was chosen randomly after researching the file values, it can be adjusted if needed

        public byte[] preData;

        public KBD(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //get text positions
                var entries = new List<TextClass>();
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var check = br.ReadUInt16();
                    foreach (var ident in textIdent)
                        if (check == ident)
                            entries.Add(new TextClass
                            {
                                entryOffset = (uint)br.BaseStream.Position - 2,
                                ident = ident
                            });
                }

                //checking for "false" idents
                for (int i = 0; i < entries.Count; i++)
                {
                    br.BaseStream.Position = entries[i].entryOffset + 2;

                    br.ReadUInt16();
                    if (entries[i].ident == 0x38)
                        br.ReadUInt16();

                    bool found = false;
                    bool falseIdent = false;
                    while (!found && !falseIdent)
                    {
                        var check = br.ReadUInt16();
                        if (check >= minLimit && check <= maxLimit)
                            found = true;
                        if (textIdent.Contains(check))
                        {
                            for (int j = 0; j < 6; j++)
                                if (br.ReadByte() == 0)
                                {
                                    falseIdent = true;
                                    break;
                                }

                            if (!falseIdent) entries.RemoveAt(i + 1);
                            if (falseIdent) entries.RemoveAt(i--);
                        }
                    }
                }
                br.BaseStream.Position = 0;

                //get more advanced data
                preData = br.ReadBytes((int)entries[0].entryOffset);
                for (int i = 0; i < entries.Count; i++)
                {
                    br.BaseStream.Position = entries[i].entryOffset + 2;

                    br.ReadUInt16();
                    if (entries[i].ident == 0x38)
                        br.ReadUInt16();

                    bool found = false;
                    while (!found)
                    {
                        var check = br.ReadUInt16();
                        if (check >= minLimit && check <= maxLimit)
                            found = true;
                    }

                    entries[i].textOffset = (uint)br.BaseStream.Position - 2;
                    br.BaseStream.Position = entries[i].entryOffset + 2;
                    entries[i].inlineData = br.ReadBytes((int)((entries[i].textOffset - 2) - (entries[i].entryOffset + 2)));

                    var stringSize = br.ReadUInt16() * 2;
                    var text = br.ReadString(stringSize, Encoding.UTF8);

                    uint nextOffset = (i + 1 == entries.Count) ? (uint)br.BaseStream.Length : entries[i + 1].entryOffset;
                    entries[i].postData = br.ReadBytes((int)(nextOffset - br.BaseStream.Position));

                    //write text and textEntry to Labels
                    Labels.Add(new Label
                    {
                        Name = $"text{i:00000000}",
                        TextID = i,
                        Text = text,
                        textEntry = entries[i]
                    });
                }
            }
        }

        public void Save(string filename)
        {
            using (var bw = new BinaryWriterX(File.OpenWrite(filename)))
            {
                //preData
                bw.Write(preData);

                //Entries (literally all other data)
                foreach (var label in Labels)
                {
                    bw.Write(label.textEntry.ident);
                    bw.Write(label.textEntry.inlineData);

                    var byteText = Encoding.UTF8.GetBytes(label.Text);
                    var byteSize = ((byteText.Length + 1) % 2 == 1) ? byteText.Length + 2 : byteText.Length + 1;

                    bw.Write((ushort)(byteSize / 2));
                    bw.Write(byteText);
                    bw.Write((byte)0);
                    bw.WriteAlignment(2);

                    bw.Write(label.textEntry.postData);
                }
            }
        }
    }
}