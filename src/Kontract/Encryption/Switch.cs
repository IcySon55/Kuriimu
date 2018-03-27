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
        private static byte[] CBC128_Decrypt(byte[] block, byte[] key, byte[] iv)
        {
            var aes = Aes.Create();

            aes.BlockSize = 128;
            aes.IV = iv;
            aes.Key = key;
            aes.Mode = CipherMode.CBC;

            var output = new byte[block.Length];
            aes.CreateDecryptor().TransformBlock(block, 0, block.Length, output, 0);
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

        private static byte[] ECB128_Decrypt(byte[] block, byte[] key)
        {
            var aes = Aes.Create();

            aes.BlockSize = 128;
            aes.Key = key;
            aes.Mode = CipherMode.ECB;

            var output = new byte[block.Length];
            aes.CreateDecryptor().TransformBlock(block, 0, block.Length, output, 0);
            return output;
        }

        private static byte[] Create_SHA256(byte[] to_hash) => SHA256.Create().ComputeHash(to_hash);

        private static byte[] xci_sha256 = new byte[] { 0x2e, 0x36, 0xcc, 0x55, 0x15, 0x7a, 0x35, 0x10, 0x90, 0xa7, 0x3e, 0x7a, 0xe7, 0x7c, 0xf5, 0x81, 0xf6, 0x9b, 0x0b, 0x6e, 0x48, 0xfb, 0x06, 0x6c, 0x98, 0x48, 0x79, 0xa6, 0xed, 0x7d, 0x2e, 0x96 };
        private static byte[] nca_sha256 = new byte[] { 0x8e, 0x03, 0xde, 0x24, 0x81, 0x8d, 0x96, 0xce, 0x4f, 0x2a, 0x09, 0xb4, 0x3a, 0xf9, 0x79, 0xe6, 0x79, 0x97, 0x4f, 0x75, 0x70, 0x71, 0x3a, 0x61, 0xee, 0xd8, 0xb3, 0x14, 0x86, 0x4a, 0x11, 0xd5 };

        public class Keyset
        {
            byte[][] masterkeys = new byte[3][];

            byte[] aesKekGenSource;
            byte[] aesKeyGenSource;

            byte[] kakAppSource;
            byte[] kakOceanSource;
            byte[] kakSystemSource;

            public byte[][][] keyAreaKeys = new byte[3][][];

            public Keyset()
            {
                masterkeys[0] = InputBox.Show($"Input Master Key #00", "Decrypt XCI").Hexlify();
                masterkeys[1] = InputBox.Show($"Input Master Key #01", "Decrypt XCI").Hexlify();
                masterkeys[2] = InputBox.Show($"Input Master Key #02", "Decrypt XCI").Hexlify();

                aesKekGenSource = InputBox.Show($"Input AES Kek Generation Source", "Decrypt XCI").Hexlify();
                aesKeyGenSource = InputBox.Show($"Input AES Key Generation Source", "Decrypt XCI").Hexlify();

                kakAppSource = InputBox.Show($"Input Key Area Key Application Source", "Decrypt XCI").Hexlify();
                kakOceanSource = InputBox.Show($"Input Key Area Key Ocean Source", "Decrypt XCI").Hexlify();
                kakSystemSource = InputBox.Show($"Input Key Area Key System Source", "Decrypt XCI").Hexlify();

                for (int i = 0; i < masterkeys.Length; i++)
                {
                    keyAreaKeys[i] = new byte[3][];
                    keyAreaKeys[i][0] = GenerateKek(kakAppSource, masterkeys[i], aesKekGenSource, aesKeyGenSource);
                    keyAreaKeys[i][1] = GenerateKek(kakOceanSource, masterkeys[i], aesKekGenSource, aesKeyGenSource);
                    keyAreaKeys[i][2] = GenerateKek(kakSystemSource, masterkeys[i], aesKekGenSource, aesKeyGenSource);
                }
            }

            private byte[] GenerateKek(byte[] generationSource, byte[] masterKey, byte[] aesKekGenSource, byte[] aesKeyGenSource)
            {
                var kek = ECB128_Decrypt(aesKekGenSource, masterKey);
                var src_kek = ECB128_Decrypt(generationSource, kek);

                if (aesKeyGenSource != null)
                {
                    return ECB128_Decrypt(aesKeyGenSource, src_kek);
                }
                else
                {
                    return src_kek;
                }
            }
        }

        public static void DecryptXCI(Stream input, Stream output, byte[] xci_header_key, byte[] nca_header_key)
        {
            if (!Create_SHA256(xci_header_key).SequenceEqual(xci_sha256))
                throw new InvalidDataException("The given XCI Header Key is wrong.");

            if (!Create_SHA256(nca_header_key).SequenceEqual(nca_sha256))
                throw new InvalidDataException("The given NCA Header Key is wrong.");

            var keyset = new Keyset();

            using (var bw = new BinaryWriterX(output, true))
            using (var br = new BinaryReaderX(input, true))
            {
                //Decrypt XCI Header
                br.BaseStream.Position = 0x120;
                var iv = br.ReadBytes(0x10).Reverse().ToArray();

                br.BaseStream.Position = 0x190;
                var encrypted_xci_header = br.ReadBytes(0x70);

                bw.BaseStream.Position = 0x190;
                bw.Write(CBC128_Decrypt(encrypted_xci_header, xci_header_key, iv));

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
                        DecryptNCA(input, output, entry.entry.offset, nca_header_key, keyset);
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
        private static void DecryptNCA(Stream input, Stream output, long offset, byte[] nca_header_key, Keyset keyset)
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

                if (!hasRightsID)
                {
                    //Decrypt keyarea
                    bw.BaseStream.Position = 0x300;
                    var keyIndex = headerPart[0x207];
                    if (keyIndex > 2)
                        throw new InvalidDataException($"NCA KeyIndex must be 0-2. Found KeyIndex: {keyIndex}");
                    bw.Write(ECB128_Decrypt(headerPart.GetElements(0x300, 0x40), keyset.keyAreaKeys[cryptoType][keyIndex]));
                }
                else
                {
                    //Decrypt title_key
                    //var title_key=Input.Show("Input Titlekey: ","Decrypt XCI");
                    /*if (ctx->tool_ctx->settings.has_titlekey)
                    {
                        aes_ctx_t* aes_ctx = new_aes_ctx(ctx->tool_ctx->settings.keyset.titlekeks[ctx->crypto_type], 16, AES_MODE_ECB);
                        aes_decrypt(aes_ctx, ctx->tool_ctx->settings.dec_titlekey, ctx->tool_ctx->settings.titlekey, 0x10);
                        free_aes_ctx(aes_ctx);
                    }*/
                }

                //Body section decryption
                /*if (ctx->tool_ctx->settings.has_contentkey) {
                ctx->section_contexts[i].aes = new_aes_ctx(ctx->tool_ctx->settings.contentkey, 16, AES_MODE_CTR);
            } else {
                if (ctx->has_rights_id) {
                    ctx->section_contexts[i].aes = new_aes_ctx(ctx->tool_ctx->settings.dec_titlekey, 16, AES_MODE_CTR);
                } else {
                    if (ctx->section_contexts[i].header->crypt_type == CRYPT_CTR || ctx->section_contexts[i].header->crypt_type == CRYPT_BKTR) {
                        ctx->section_contexts[i].aes = new_aes_ctx(ctx->decrypted_keys[2], 16, AES_MODE_CTR);
                    } else if (ctx->section_contexts[i].header->crypt_type == CRYPT_XTS) {
                        ctx->section_contexts[i].aes = new_aes_ctx(ctx->decrypted_keys[0], 32, AES_MODE_XTS);
                    }
                }
            }*/
            }
        }
        #endregion
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
