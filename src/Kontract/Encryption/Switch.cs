using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kontract.IO;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using Kontract.Encryption.AES_XTS;
using System.Windows.Forms;

namespace Kontract.Encryption
{
    public class Switch
    {
        private static Aes Initialize_CBC128(byte[] key, byte[] iv)
        {
            var aes = Aes.Create();

            aes.BlockSize = 128;
            aes.IV = iv;
            aes.Key = key;
            aes.Mode = CipherMode.CBC;

            return aes;
        }
        private static byte[] CBC128_Decrypt(Aes cbc128, byte[] block)
        {
            var output = new byte[block.Length];
            cbc128.CreateDecryptor().TransformBlock(block, 0, block.Length, output, 0);
            return output;
        }

        private static byte[] XTS128_Decrypt(byte[] block, byte[] key, int block_size, int section_id = 0)
        {
            var result = new byte[block.Length];

            var xts = XtsAes128.Create(key, true);
            using (var transform = xts.CreateDecryptor())
            {
                for (int i = 0; i < block.Length / block_size; i++)
                {
                    transform.TransformBlock(block, i * block_size, block_size, result, i * block_size, Convert.ToUInt64(section_id++));
                }
            }

            return result;
        }

        private static byte[] ECB_Decrypt(byte[] block, byte[] key)
        {
            return null;
        }

        private static byte[] Create_SHA256(byte[] to_hash) => SHA256.Create().ComputeHash(to_hash);

        private static byte[] xci_sha256 = new byte[] { 0x2e, 0x36, 0xcc, 0x55, 0x15, 0x7a, 0x35, 0x10, 0x90, 0xa7, 0x3e, 0x7a, 0xe7, 0x7c, 0xf5, 0x81, 0xf6, 0x9b, 0x0b, 0x6e, 0x48, 0xfb, 0x06, 0x6c, 0x98, 0x48, 0x79, 0xa6, 0xed, 0x7d, 0x2e, 0x96 };
        private static byte[] nca_sha256 = new byte[] { 0x8e, 0x03, 0xde, 0x24, 0x81, 0x8d, 0x96, 0xce, 0x4f, 0x2a, 0x09, 0xb4, 0x3a, 0xf9, 0x79, 0xe6, 0x79, 0x97, 0x4f, 0x75, 0x70, 0x71, 0x3a, 0x61, 0xee, 0xd8, 0xb3, 0x14, 0x86, 0x4a, 0x11, 0xd5 };

        public static void DecryptXCI(Stream input, Stream output, byte[] xci_header_key, byte[] nca_header_key)
        {
            if (!Create_SHA256(xci_header_key).SequenceEqual(xci_sha256))
                throw new InvalidDataException("The given XCI Header Key is wrong.");

            if (!Create_SHA256(nca_header_key).SequenceEqual(nca_sha256))
                throw new InvalidDataException("The given NCA Header Key is wrong.");

            using (var bw = new BinaryWriterX(output, true))
            using (var br = new BinaryReaderX(input, true))
            {
                //Decrypt XCI Header
                br.BaseStream.Position = 0x120;
                var iv = br.ReadBytes(0x10).Reverse().ToArray();
                var aes_cbc128 = Initialize_CBC128(xci_header_key, iv);

                br.BaseStream.Position = 0x190;
                var encrypted_xci_header = br.ReadBytes(0x70);

                bw.BaseStream.Position = 0x190;
                bw.Write(CBC128_Decrypt(aes_cbc128, encrypted_xci_header));

                //Get HFS0 entries for NCA's
                br.BaseStream.Position = 0x130;
                var hfs0_offset = br.ReadInt64();
                var hfs0_header_size = br.ReadInt64();
                var ncaEntries = ParseHFS0List(br.BaseStream, hfs0_offset);

                //Decrypt NCA's
                foreach (var entry in ncaEntries)
                {
                    if (entry.name.Contains(".nca"))
                    {
                        DecryptNCA(input, output, entry.entry.offset, nca_header_key);
                    }
                }
            }
        }

        #region HFS0
        private static List<HFS0NamedEntry> ParseHFS0List(Stream input, long hfs0_offset)
        {
            var baseEntries = ParseHFS0Partition(input, hfs0_offset);
            var result = new List<HFS0NamedEntry>();
            foreach (var entry in baseEntries)
                result.AddRange(ParseHFS0Partition(input, entry.entry.offset));

            return result;
        }

        private static List<HFS0NamedEntry> ParseHFS0Partition(Stream input, long hfs0_offset)
        {
            var namedEntries = new List<HFS0NamedEntry>();
            long hfs0_header_size = 0;

            using (var br = new BinaryReaderX(input, true))
            {
                br.BaseStream.Position = hfs0_offset;
                var header = br.ReadStruct<HFS0Header>();
                var entries = br.ReadMultiple<HFS0Entry>(header.fileCount);
                var stringTable = br.ReadBytes(header.stringTableSize);

                hfs0_header_size = 0x10 + header.fileCount * 0x40 + header.stringTableSize;

                using (var nameBr = new BinaryReaderX(new MemoryStream(stringTable)))
                    foreach (var entry in entries)
                    {
                        nameBr.BaseStream.Position = entry.nameOffset;
                        namedEntries.Add(new HFS0NamedEntry { name = nameBr.ReadCStringA(), entry = entry });
                    }
            }

            //Absolutize offsets
            foreach (var entry in namedEntries)
                entry.entry.offset += hfs0_offset + hfs0_header_size;

            return namedEntries;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class HFS0Header
        {
            public Magic magic;
            public int fileCount;
            public int stringTableSize;
            public int reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class HFS0Entry
        {
            public long offset;
            public long size;
            public int nameOffset;
            public int hashedSize;
            public long reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
            public byte[] hash;
        }

        public class HFS0NamedEntry
        {
            public string name;
            public HFS0Entry entry;
        }
        #endregion

        #region NCA Decryption
        private static void DecryptNCA(Stream input, Stream output, long offset, byte[] nca_header_key)
        {
            using (var bw = new BinaryWriterX(output, true))
            using (var br = new BinaryReaderX(input, true))
            {
                br.BaseStream.Position = offset;
                bw.BaseStream.Position = offset;

                //Decrypt NCA Header
                var headerPart = XTS128_Decrypt(br.ReadBytes(0x400), nca_header_key, 0x200);
                var magic = headerPart.GetElements(0x100, 4).Aggregate("", (o, b) => o + (char)b);
                bw.Write(headerPart);
                if (magic == "NCA3")
                {
                    bw.Write(XTS128_Decrypt(br.ReadBytes(0x800), nca_header_key, 0x200, 2));
                }
                else if (magic == "NCA2")
                {
                    for (int i = 0; i < 4; i++)
                        bw.Write(XTS128_Decrypt(br.ReadBytes(0x200), nca_header_key, 0x200));
                }

                //Get crypto_type
                var cryptoType = (headerPart[0x220] > headerPart[0x206]) ? headerPart[0x220] : headerPart[0x206];
                if (cryptoType == 1) cryptoType--;

                //RightsID
                bool hasRightsID = false;
                for (int i = 0; i < 0x10; i++)
                    if (headerPart[0x230 + i] != 0)
                    {
                        hasRightsID = true;
                        break;
                    }

                if (hasRightsID)
                {
                    //Decrypt keyarea
                    var decKeyArea = ECB_Decrypt(headerPart.GetElements(0x300, 0x40), null);
                }
                else
                {
                    //Decrypt title_key
                }
            }
            #endregion
        }
    }

    static class Support
    {
        public static byte[] GetElements(this byte[] input, int index, int length)
        {
            var result = new byte[length];
            for (int i = index; i < index + length; i++)
                result[i - index] = input.ElementAt(i);
            return result;
        }
    }
}