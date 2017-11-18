using Kontract.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Komponent.IO;

namespace archive_rgssad
{
    public class RGSSAD
    {
        public List<RGSSADFileInfo> Files = new List<RGSSADFileInfo>();

        private Stream _rgssadStream = null;

        public RGSSAD(Stream rgssadStream)
        {
            _rgssadStream = rgssadStream;

            using (var br = new BinaryReaderX(rgssadStream, true))
            {
                var header = br.ReadCStringA();
                var version = (RGSSADVersion)br.ReadByte();

                if (version == RGSSADVersion.RGSSADv1)
                {
                    var archive = new RGSSADv1(br);

                    Files = archive.Entries.Select(o => new RGSSADFileInfo
                    {
                        Entry = o,
                        FileName = o.Name,
                        FileData = new MemoryStream(o.Data),
                        State = ArchiveFileState.Archived
                    }).ToList();
                }
                else if (version == RGSSADVersion.RGSSADv3)
                {
                    var archive = new RGSSADv3(br);

                    Files = archive.Entries.Select(o => new RGSSADFileInfo
                    {
                        Entry = o,
                        FileName = o.Name,
                        FileData = new MemoryStream(o.Data),
                        State = ArchiveFileState.Archived
                    }).ToList();
                }
            }
        }

        public void Close()
        {
            _rgssadStream?.Close();
            _rgssadStream = null;

            foreach (var afi in Files)
                if (afi.State != ArchiveFileState.Archived)
                    afi.FileData?.Dispose();
        }
    }

    public class RGSSADv1
    {
        public List<RGSSADFileEntry> Entries = new List<RGSSADFileEntry>();

        public RGSSADv1(BinaryReaderX br)
        {
            // Key for the RGSSADv1 format initilized as a constant
            uint key = 0xDEADCAFE;

            br.BaseStream.Position = 8;
            while (true)
            {
                RGSSADFileEntry entry = new RGSSADFileEntry();

                int length = decryptInt(br.ReadInt32(), ref key);
                entry.Name = decryptFilename(br.ReadBytes(length), ref key);
                entry.Size = decryptInt(br.ReadInt32(), ref key);
                entry.Offset = br.BaseStream.Position;
                entry.Key = key;
                entry.Data = RGSSADSupport.decryptFileData(br.ReadBytes(entry.Size), entry.Key);
                Entries.Add(entry);

                if (br.BaseStream.Position == br.BaseStream.Length)
                    break;
            }
        }

        private int decryptInt(int value, ref uint key)
        {
            long result = value ^ key;

            key *= 7;
            key += 3;

            return (int)result;
        }

        private string decryptFilename(byte[] name, ref uint key)
        {
            byte[] decrypted = new byte[name.Length];

            for (int i = 0; i < name.Length; ++i)
            {
                decrypted[i] = (byte)(name[i] ^ (key & 0xFF));

                key *= 7;
                key += 3;
            }

            return Encoding.UTF8.GetString(decrypted);
        }
    }

    public class RGSSADv3
    {
        public List<RGSSADFileEntry> Entries = new List<RGSSADFileEntry>();

        public RGSSADv3(BinaryReaderX br)
        {
            br.BaseStream.Position = 8;

            uint key = (uint)br.ReadInt32();
            key *= 9;
            key += 3;

            while (true)
            {
                RGSSADFileEntry entry = new RGSSADFileEntry();
                entry.Offset = decryptInt(br.ReadInt32(), key);
                entry.Size = decryptInt(br.ReadInt32(), key);
                entry.Key = (uint)decryptInt(br.ReadInt32(), key);

                int length = decryptInt(br.ReadInt32(), key);

                if (entry.Offset == 0)
                    break;

                entry.Name = decryptFilename(br.ReadBytes(length), key);

                var tempPosition = br.BaseStream.Position;

                br.BaseStream.Position = entry.Offset;
                entry.Data = RGSSADSupport.decryptFileData(br.ReadBytes(entry.Size), entry.Key);
                br.BaseStream.Position = tempPosition;

                Entries.Add(entry);
            }
        }

        private int decryptInt(int value, uint key)
        {
            long result = value ^ key;
            return (int)result;
        }

        private string decryptFilename(byte[] name, uint key)
        {
            byte[] decrypted = new byte[name.Length];

            byte[] keyBytes = BitConverter.GetBytes(key);

            int j = 0;
            for (int i = 0; i < name.Length; ++i)
            {
                if (j == 4)
                    j = 0;
                decrypted[i] = (byte)(name[i] ^ keyBytes[j]);
                j += 1;
            }

            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
