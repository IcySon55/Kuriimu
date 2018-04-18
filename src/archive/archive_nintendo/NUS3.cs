using System.Collections.Generic;
using System.IO;
using Kontract.Compression;
using Kontract.Interface;
using Kontract.IO;

namespace archive_nintendo.NUS3
{
    public sealed class NUS3
    {
        public List<NUS3FileInfo> Files = new List<NUS3FileInfo>();

        // Header
        private Header Header;
        private BankTOC BankTOC;
        private List<SectionHeader> Sections;

        // Sections
        private PROP Prop;
        private BINF Binf;
        private GRP Grp;
        private DTON Dton;
        private TONE Tone;
        private JUNK Junk;
        private SectionHeader Pack;

        Stream _stream = null;

        public NUS3(string filename, bool isZlibCompressed = false)
        {
            // Compression
            if (isZlibCompressed)
            {
                using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename)))
                {
                    byte[] decomp = ZLib.Decompress(new MemoryStream(br.ReadBytes((int)br.BaseStream.Length)));
                    File.Create(filename + ".decomp").Write(decomp, 0, decomp.Length);
                }

                _stream = File.OpenRead(filename + ".decomp");
            }
            else
                _stream = File.OpenRead(filename);

            using (var br = new BinaryReaderX(_stream, true))
            {
                // Header
                Header = br.ReadStruct<Header>();

                // BankTOC
                BankTOC = br.ReadStruct<BankTOC>();

                // Sections
                Sections = br.ReadMultiple<SectionHeader>(BankTOC.SectionCount);

                // Load Sections
                foreach (var section in Sections)
                {
                    switch (section.Magic)
                    {
                        case "PROP":
                            Prop = new PROP(br.BaseStream);
                            break;
                        case "BINF":
                            Binf = new BINF(br.BaseStream);
                            break;
                        case "GRP ":
                            Grp = new GRP(br.BaseStream);
                            break;
                        case "DTON":
                            Dton = new DTON(br.BaseStream);
                            break;
                        case "TONE":
                            Tone = new TONE(br.BaseStream);
                            break;
                        case "JUNK":
                            Junk = new JUNK(br.BaseStream);
                            break;
                        case "PACK":
                            Pack = br.ReadStruct<SectionHeader>();
                            break;
                    }
                }

                // Pack
                var packOffset = br.BaseStream.Position;
                for (int i = 0; i < Tone.ToneCount; i++)
                {
                    // Determine Extension
                    var extension = ".idsp";
                    if (br.PeekString(4) == "RIFF")
                    {
                        extension = ".wav";
                        br.BaseStream.Position += 0x14;
                        var format = br.ReadUInt16();
                        if (format == 0xFFFE)
                            extension = ".at9";
                    }

                    Files.Add(new NUS3FileInfo
                    {
                        FileName = Tone.Name + extension,
                        FileData = new SubStream(_stream, packOffset, Tone.PackSize),
                        State = ArchiveFileState.Archived
                    });
                }
            }
        }

        public void Save(string filename, bool isZLibCompressed = false)
        {
            using (var bw = new BinaryWriterX(File.Create(filename)))
            {
                bw.BaseStream.Position = 0x18 + (BankTOC.SectionCount * 0x8);

                // Save Sections
                foreach (var section in Sections)
                {
                    var startOffset = bw.BaseStream.Position;
                    bw.BaseStream.Position += 0x8;

                    switch (section.Magic)
                    {
                        case "PROP":
                            Prop.Save(bw.BaseStream);
                            break;
                        case "BINF":
                            Binf.Save(bw.BaseStream);
                            break;
                        case "GRP ":
                            Grp.Save(bw.BaseStream);
                            break;
                        case "DTON":
                            Dton.Save(bw.BaseStream);
                            break;
                        case "TONE":
                            Tone.PackSize = (int)Files[0].FileSize;
                            Tone.Save(bw.BaseStream);
                            break;
                        case "JUNK":
                            Junk.Save(bw.BaseStream);
                            break;
                        case "PACK":
                            Files[0].FileData.CopyTo(bw.BaseStream);
                            //for (int i = 0; i < Tone.ToneCount; i++)
                            //{
                            //}
                            break;
                    }

                    section.SectionSize = (int)(bw.BaseStream.Position - (startOffset + 0x8));
                    bw.BaseStream.Position = startOffset;
                    bw.WriteStruct(section);
                    bw.BaseStream.Position += section.SectionSize;
                }

                // FIleSize
                Header.FileSize = (int)bw.BaseStream.Position - 0x8;
                bw.BaseStream.Position = 0;
                bw.WriteStruct(Header);

                // BankTOC
                bw.WriteStruct(BankTOC);

                // Sections
                foreach (var section in Sections)
                    bw.WriteStruct(section);
            }

            if (isZLibCompressed)
            {
                FileStream origFile = File.OpenRead(filename);
                byte[] decomp = new byte[(int)origFile.Length];
                origFile.Read(decomp, 0, (int)origFile.Length);
                byte[] comp = ZLib.Compress(new MemoryStream(decomp));

                origFile.Close();
                File.Delete(filename);
                File.OpenWrite(filename).Write(comp, 0, comp.Length);
            }
        }

        public void Close()
        {
            _stream?.Close();
            _stream = null;
        }
    }
}
