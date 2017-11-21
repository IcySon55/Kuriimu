using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Kontract.Interface;
using System.IO;
using Komponent.IO;
using Komponent.Compression;
using System;

namespace archive_cdar
{
    public class CDARFileInfo : ArchiveFileInfo
    {
        public uint hash;
        public Entry entry;

        public override Stream FileData
        {
            get
            {
                if (State != ArchiveFileState.Archived || entry.decompSize == 0) return base.FileData;
                return new MemoryStream(new ZLib().Decompress(base.FileData, 0));
            }
        }

        public override long? FileSize => (entry.decompSize == 0) ? entry.compSize : entry.decompSize;

        public void Write(Stream input, uint offset)
        {
            using (var bw = new BinaryWriterX(input, true))
            {
                bw.BaseStream.Position = offset;

                if (State == ArchiveFileState.Archived)
                {
                    base.FileData.CopyTo(bw.BaseStream);
                    bw.WriteAlignment(0x10, (byte)new Random().Next());
                }
                else
                {
                    if (entry.decompSize == 0)
                    {
                        entry.offset = offset;
                        entry.compSize = (uint)base.FileData.Length;
                        base.FileData.CopyTo(bw.BaseStream);

                        bw.WriteAlignment(0x10, (byte)new Random().Next());
                    }
                    else
                    {
                        entry.offset = offset;
                        entry.decompSize = (uint)base.FileData.Length;
                        var compM = new ZLib();
                        compM.SetMethod(0);
                        var comp = compM.Compress(base.FileData);
                        entry.compSize = (uint)comp.Length;
                        bw.Write(comp);

                        bw.WriteAlignment(0x10, (byte)new Random().Next());
                    }
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        public Magic magic;
        public uint unk1;
        public uint entryCount;
        public uint unk2;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Entry
    {
        public uint offset;
        public uint decompSize;
        public uint compSize;
    }
}
