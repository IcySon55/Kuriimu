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
using Kontract.CTR;

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

        private static byte[] CTR128_Decrypt(byte[] block, byte[] key, byte[] ctr)
        {
            var aes = new AesCtr(ctr);

            var output = new byte[block.Length];
            aes.CreateDecryptor(key).TransformBlock(block, 0, block.Length, output, 0);
            return output;
        }

        private static byte[] Create_SHA256(byte[] to_hash) => SHA256.Create().ComputeHash(to_hash);

        public class Keyset
        {
            private static byte[] ncaHeader_sha256 = new byte[] { 0x8e, 0x03, 0xde, 0x24, 0x81, 0x8d, 0x96, 0xce, 0x4f, 0x2a, 0x09, 0xb4, 0x3a, 0xf9, 0x79, 0xe6, 0x79, 0x97, 0x4f, 0x75, 0x70, 0x71, 0x3a, 0x61, 0xee, 0xd8, 0xb3, 0x14, 0x86, 0x4a, 0x11, 0xd5 };

            public byte[] ncaHeaderKey = new byte[0x20];

            byte[][] masterkeys = new byte[3][];
            byte[] titlekekSource;

            byte[] aesKekGenSource;
            byte[] aesKeyGenSource;

            byte[] kakAppSource;
            byte[] kakOceanSource;
            byte[] kakSystemSource;

            public byte[][][] keyAreaKeys = new byte[3][][];
            public byte[][] titleKeks = new byte[3][];

            public Keyset()
            {
                if (File.Exists("bin\\switch_keys.dat"))
                {
                    //use keyfile, if found
                    var lines = File.ReadAllLines("bin\\switch_keys.dat");
                    foreach (var line in lines)
                    {
                        if (line != String.Empty)
                        {
                            var key_desc = line.Replace(" ", "");
                            var name = key_desc.Split(new[] { ':', '=' })[0];
                            var key = key_desc.Split(new[] { ':', '=' })[1];

                            if (name.Contains("master_key"))
                                masterkeys[Convert.ToInt32(name.Split(new string[] { "master_key_" }, StringSplitOptions.None)[1])] = key.Hexlify(16);
                            if (name == "header_key")
                                ncaHeaderKey = key.Hexlify(32);
                            if (name == "title_kek_source")
                                titlekekSource = key.Hexlify(16);
                            if (name == "aes_kek_generation_source")
                                aesKekGenSource = key.Hexlify(16);
                            if (name == "aes_key_generation_source")
                                aesKeyGenSource = key.Hexlify(16);
                            if (name == "key_area_key_application_source")
                                kakAppSource = key.Hexlify(16);
                            if (name == "key_area_key_ocean_source")
                                kakOceanSource = key.Hexlify(16);
                            if (name == "key_area_key_system_source")
                                kakSystemSource = key.Hexlify(16);
                        }
                    }
                }
                else
                {
                    //else ask for every key
                    ncaHeaderKey = InputBox.Show($"Input NCA Header Key", "Decrypt XCI").Hexlify(32);
                    if (!Create_SHA256(ncaHeaderKey).SequenceEqual(ncaHeader_sha256))
                        throw new InvalidDataException("The given NCA Header Key is wrong.");

                    masterkeys[0] = InputBox.Show($"Input Master Key #00", "Decrypt XCI").Hexlify(16);
                    masterkeys[1] = InputBox.Show($"Input Master Key #01", "Decrypt XCI").Hexlify(16);
                    masterkeys[2] = InputBox.Show($"Input Master Key #02", "Decrypt XCI").Hexlify(16);

                    titlekekSource = InputBox.Show($"Input Title Kek Source", "Decrypt XCI").Hexlify(16);

                    aesKekGenSource = InputBox.Show($"Input AES Kek Generation Source", "Decrypt XCI").Hexlify(16);
                    aesKeyGenSource = InputBox.Show($"Input AES Key Generation Source", "Decrypt XCI").Hexlify(16);

                    kakAppSource = InputBox.Show($"Input Key Area Key Application Source", "Decrypt XCI").Hexlify(16);
                    kakOceanSource = InputBox.Show($"Input Key Area Key Ocean Source", "Decrypt XCI").Hexlify(16);
                    kakSystemSource = InputBox.Show($"Input Key Area Key System Source", "Decrypt XCI").Hexlify(16);
                }

                for (int i = 0; i < masterkeys.Length; i++)
                {
                    keyAreaKeys[i] = new byte[3][];
                    keyAreaKeys[i][0] = GenerateKek(kakAppSource, masterkeys[i], aesKekGenSource, aesKeyGenSource);
                    keyAreaKeys[i][1] = GenerateKek(kakOceanSource, masterkeys[i], aesKekGenSource, aesKeyGenSource);
                    keyAreaKeys[i][2] = GenerateKek(kakSystemSource, masterkeys[i], aesKekGenSource, aesKeyGenSource);

                    titleKeks[i] = ECB128_Decrypt(titlekekSource, masterkeys[i]);
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

        private static byte[] xci_sha256 = new byte[] { 0x2e, 0x36, 0xcc, 0x55, 0x15, 0x7a, 0x35, 0x10, 0x90, 0xa7, 0x3e, 0x7a, 0xe7, 0x7c, 0xf5, 0x81, 0xf6, 0x9b, 0x0b, 0x6e, 0x48, 0xfb, 0x06, 0x6c, 0x98, 0x48, 0x79, 0xa6, 0xed, 0x7d, 0x2e, 0x96 };

        public static void DecryptXCI(Stream input, Stream output, byte[] xci_header_key)
        {
            if (!Create_SHA256(xci_header_key).SequenceEqual(xci_sha256))
                throw new InvalidDataException("The given XCI Header Key is wrong.");

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
                        DecryptNCA(input, output, entry.entry.offset);
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
        private static void DecryptNCA(Stream input, Stream output, long offset)
        {
            var keyset = new Keyset();

            using (var bw = new BinaryWriterX(output, true))
            using (var br = new BinaryReaderX(input, true))
            {
                br.BaseStream.Position = offset;
                bw.BaseStream.Position = offset;

                //Decrypt NCA Header
                var headerPart = XTS128_Decrypt(br.ReadBytes(0x400), keyset.ncaHeaderKey, 0x200);
                var magic = headerPart.GetElements(0x200, 4).Aggregate("", (o, b) => o + (char)b);
                bw.Write(headerPart);
                var headerPart2 = new byte[0x800];
                if (magic == "NCA3")
                {
                    headerPart2 = XTS128_Decrypt(br.ReadBytes(0x800), keyset.ncaHeaderKey, 0x200, 2);
                    bw.Write(headerPart2);
                }
                else if (magic == "NCA2")
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var buffer = XTS128_Decrypt(br.ReadBytes(0x200), keyset.ncaHeaderKey, 0x200);
                        Array.Copy(buffer, 0, headerPart2, i * 0x200, 0x200);
                        bw.Write(buffer);
                    }
                } else
                {
                    throw new InvalidDataException("Invalid NCA Header! Are the keys correct?");
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

                /* Explanation:
                 * - if a RightsID is set, an external titleKey is needed. This titleKey is contained in a ticket,
                 *   most likely installed on the system by download titles from eshop
                 * - if no RightsID is set, the nca internal keyarea is used to decrypt the contents
                 */

                byte[] dec_title_key = null;
                byte[] dec_key_area = null;
                if (!hasRightsID)
                {
                    //Decrypt keyarea
                    bw.BaseStream.Position = offset + 0x300;
                    var keyIndex = headerPart[0x207];
                    if (keyIndex > 2)
                        throw new InvalidDataException($"NCA KeyIndex must be 0-2. Found KeyIndex: {keyIndex}");
                    dec_key_area = ECB128_Decrypt(headerPart.GetElements(0x300, 0x40), keyset.keyAreaKeys[cryptoType][keyIndex]);
                    bw.Write(dec_key_area);
                }
                else
                {
                    //Decrypt title_key
                    var title_key = InputBox.Show("Input Titlekey:", "Decrypt NCA").Hexlify(16);
                    dec_title_key = ECB128_Decrypt(title_key, keyset.titleKeks[cryptoType]);
                }

                //Read out section crypto
                List<SectionEntry> sectionList = new BinaryReaderX(new MemoryStream(headerPart.GetElements(0x240, 0x40))).ReadMultiple<SectionEntry>(4);
                for (int i = 0; i < 4; i++)
                {
                    if (sectionList[i].mediaOffset != 0 && sectionList[i].endMediaOffset != 0)
                    {
                        br.BaseStream.Position = offset + sectionList[i].mediaOffset * 0x200;
                        bw.BaseStream.Position = offset + sectionList[i].mediaOffset * 0x200;

                        if (hasRightsID)
                        {
                            var enc_buffer = br.ReadBytes(sectionList[i].endMediaOffset * 0x200 - sectionList[i].mediaOffset * 0x200);
                            var dec_buffer = CTR128_Decrypt(enc_buffer, dec_title_key, GenerateCTR(sectionList[i].mediaOffset * 0x200));
                            bw.Write(dec_buffer);
                        }
                        else
                        {
                            var sectionCrypto = headerPart2[i * 0x200 + 0x4];
                            if (sectionCrypto == 0 || sectionCrypto > 4)
                                throw new InvalidDataException($"SectionCrypto {i} must be 1-4. Found SectionCrypto: {sectionCrypto}");

                            switch (sectionCrypto)
                            {
                                //case 1 is NoCrypto
                                case 2:
                                    //XTS
                                    var enc_buffer = br.ReadBytes(sectionList[i].endMediaOffset * 0x200 - sectionList[i].mediaOffset * 0x200);
                                    var dec_buffer = XTS128_Decrypt(enc_buffer, dec_key_area.GetElements(0, 0x20), 0x200);
                                    bw.Write(dec_buffer);
                                    break;
                                case 3:
                                    //CTR
                                    enc_buffer = br.ReadBytes(sectionList[i].endMediaOffset * 0x200 - sectionList[i].mediaOffset * 0x200);
                                    dec_buffer = CTR128_Decrypt(enc_buffer, dec_key_area.GetElements(0x20, 0x10), GenerateCTR(sectionList[i].mediaOffset * 0x200));
                                    bw.Write(dec_buffer);
                                    break;
                                case 4:
                                    //BKTR
                                    //stub
                                    break;
                            }
                        }
                    }
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

        private static byte[] GenerateCTR(long offset)
        {
            offset >>= 4;
            byte[] ctr = new byte[0x10];
            for (int i = 0; i < 8; i++)
            {
                ctr[0x10 - i - 1] = (byte)(offset & 0xFF);
                offset >>= 8;
            }
            return ctr;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private class SectionEntry
        {
            public int mediaOffset;
            public int endMediaOffset;
            public int unk1;
            public int unk2;
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
