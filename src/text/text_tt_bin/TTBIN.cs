using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kontract.IO;

namespace tt.text_ttbin
{
    public sealed class TTBIN
    {
        public List<Label> Labels = new List<Label>();

        public Header cfgHeader;
        public List<EditorStruct> editorEntries = new List<EditorStruct>();
        public List<TextStruct> textEntries = new List<TextStruct>();
        public byte[] editorRest;
        public byte[] textRest;

        public byte type = 0;
        public int labelCount;

        public TTBIN(string filename)
        {
            long bk;

            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                //Header
                cfgHeader = br.ReadStruct<Header>();

                editorEntries.Add(br.ReadStruct<EditorStruct>());

                //getting Editor's notes entries
                while (true)
                {
                    Label label = new Label();

                    EditorStruct entryE = br.ReadStruct<EditorStruct>();
                    editorEntries.Add(entryE);

                    label.Name = "editor" + (editorEntries.Count - 1).ToString();
                    label.TextID = entryE.ID;
                    label.TextOffset = cfgHeader.dataOffset + entryE.entryOffset;

                    bk = br.BaseStream.Position;
                    br.BaseStream.Position = label.TextOffset;
                    string text = ""; byte part = br.ReadByte();
                    while (part != 0)
                    {
                        text += Encoding.GetEncoding("ascii").GetString(new byte[] { part });
                        part = br.ReadByte();
                    }
                    br.BaseStream.Position = label.TextOffset;
                    label.Text = text;
                    br.BaseStream.Position = bk;

                    Labels.Add(label);

                    if (entryE.endingFlag == 0x0101)
                    {
                        break;
                    }
                }

                bool found = false;
                bk = br.BaseStream.Position;
                while (br.BaseStream.Position < cfgHeader.dataOffset && found == false)
                {
                    if (br.ReadInt32() == (int)(editorEntries[editorEntries.Count - 1].entryOffset + Labels[Labels.Count - 1].Text.Length + 1))
                    {
                        found = true;
                    }
                }

                br.BaseStream.Position = bk;
                if (found == false)
                {
                    editorRest = br.ReadBytes((int)(cfgHeader.dataOffset - br.BaseStream.Position));
                    textRest = null;
                    textEntries = null;
                }
                else
                {
                    editorRest = null;

                    textEntries.Add(br.ReadStruct<TextStruct>());

                    //getting text entries
                    TextStruct entryT;
                    TextStruct entryT2;
                    int entryCount = 1;
                    do
                    {
                        Label label = new Label();

                        entryT = br.ReadStruct<TextStruct>();
                        textEntries.Add(entryT);

                        entryT2 = br.ReadStruct<TextStruct>();
                        br.BaseStream.Position -= 0x14;

                        if (entryT.entryOffset != 0xFFFFFFFF)
                        {
                            label.Name = "text" + entryCount.ToString(); entryCount++;
                            label.TextID = entryT.ID;
                            label.TextOffset = cfgHeader.dataOffset + entryT.entryOffset;

                            bk = br.BaseStream.Position;
                            br.BaseStream.Position = label.TextOffset;
                            int count = 0; byte part = br.ReadByte();
                            while (part != 0)
                            {
                                count++;
                                part = br.ReadByte();
                            }
                            count++;
                            br.BaseStream.Position = label.TextOffset;
                            label.Text = getUnicodeString(new BinaryReaderX(new MemoryStream(br.ReadBytes(count))));
                            br.BaseStream.Position = bk;

                            Labels.Add(label);
                        }
                    } while (entryT.unk3 != 0xffffff00 && (entryT2.entryOffset <= br.BaseStream.Length || entryT2.entryOffset == 0xFFFFFFFF));

                    if (br.BaseStream.Position < cfgHeader.dataOffset) textRest = br.ReadBytes((int)cfgHeader.dataOffset - (int)br.BaseStream.Position);
                }
            }
        }

        public static string getUnicodeString(BinaryReaderX br)
        {
            Encoding sjis = Encoding.GetEncoding("shift-jis");
            string uni = ""; byte part;

            do
            {
                part = br.ReadByte();
                uni += (part >= 0x80) ? sjis.GetString(new byte[] { part, br.ReadByte() }) : (part > 0x00) ? sjis.GetString(new byte[] { part }) : "";
            } while (part != 0x00);
            return uni;
        }

        public void Save(string filename)
        {
            long bk;

            using (var bw = new BinaryWriterX(File.Create(filename)))
            {
                bw.WriteStruct(cfgHeader);

                for (int j = 0; j < editorEntries.Count; j++)
                {
                    if (j > 0)
                    {
                        EditorStruct bk2 = editorEntries[j];

                        var success1 = (cfgHeader.dataOffset > bw.BaseStream.Length) ? bk2.entryOffset = 0 : bk2.entryOffset = (uint)bw.BaseStream.Length - cfgHeader.dataOffset;

                        bk = bw.BaseStream.Position;
                        var success2 = (cfgHeader.dataOffset > bw.BaseStream.Length) ? bw.BaseStream.Position = cfgHeader.dataOffset : bw.BaseStream.Position = bw.BaseStream.Length;
                        bw.Write(Encoding.GetEncoding("shift-jis").GetBytes(Labels[labelCount++].Text));
                        bw.Write((byte)0x00);
                        bw.BaseStream.Position = bk;

                        editorEntries[j] = bk2;
                    }

                    bw.WriteStruct<EditorStruct>(editorEntries[j]);
                }

                if (editorRest != null) bw.Write(editorRest);
                else
                {
                    for (int j = 0; j < textEntries.Count; j++)
                    {
                        if (j > 0 && textEntries[j].entryOffset != 0xFFFFFFFF)
                        {
                            TextStruct bk2 = textEntries[j];

                            var success1 = (cfgHeader.dataOffset > bw.BaseStream.Length) ? bk2.entryOffset = 0 : bk2.entryOffset = (uint)bw.BaseStream.Length - cfgHeader.dataOffset;

                            bk = bw.BaseStream.Position;
                            var success2 = (cfgHeader.dataOffset > bw.BaseStream.Length) ? bw.BaseStream.Position = cfgHeader.dataOffset : bw.BaseStream.Position = bw.BaseStream.Length;
                            bw.Write(Encoding.GetEncoding("shift-jis").GetBytes(Labels[labelCount++].Text));
                            bw.Write((byte)0x00);
                            bw.BaseStream.Position = bk;

                            textEntries[j] = bk2;
                        }

                        bw.WriteStruct(textEntries[j]);
                    }
                    if (textRest != null) bw.Write(textRest);
                }

                bw.BaseStream.Position = 0x8;
                bw.Write((int)(bw.BaseStream.Length - cfgHeader.dataOffset));

                bw.BaseStream.Position = bw.BaseStream.Length;
                bw.WriteAlignment(0x10, 0xff);
            }
        }
    }
}
