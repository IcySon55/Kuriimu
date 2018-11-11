using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Kontract.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Text.RegularExpressions;

using Kontract.Encryption.AES;
using Kontract.Hash;

namespace Kontract.Encryption
{
    public class Switch
    {
        private const long _mediaUnit = 0x200;

        public class Keyset
        {
            private Dictionary<string, byte[]> _keyMaterial;

            public byte[] this[string i]
            {
                get
                {
                    if (!_keyMaterial.ContainsKey(i))
                        throw new KeyNotFoundException(i);
                    return _keyMaterial[i];
                }
                set
                {
                    if (!_keyMaterial.ContainsKey(i))
                        _keyMaterial.Add(i, value);
                    else
                        _keyMaterial[i] = value;
                }
            }

            //private static byte[] ncaHeader_sha256 = new byte[] { 0x8e, 0x03, 0xde, 0x24, 0x81, 0x8d, 0x96, 0xce, 0x4f, 0x2a, 0x09, 0xb4, 0x3a, 0xf9, 0x79, 0xe6, 0x79, 0x97, 0x4f, 0x75, 0x70, 0x71, 0x3a, 0x61, 0xee, 0xd8, 0xb3, 0x14, 0x86, 0x4a, 0x11, 0xd5 };

            //public byte[] ncaHeaderKey = new byte[0x20];

            //byte[][] masterkeys = new byte[3][];
            //byte[] titlekekSource;

            //byte[] aesKekGenSource;
            //byte[] aesKeyGenSource;

            //byte[] kakAppSource;
            //byte[] kakOceanSource;
            //byte[] kakSystemSource;

            public Keyset()
            {
                if (!File.Exists("bin\\switch_keys.dat"))
                    throw new FileNotFoundException("Couldn't find switch_keys.dat");

                _keyMaterial = File.ReadAllLines("bin\\switch_keys.dat")
                    .Select(l => l.Replace(" ", "").Replace("\t", ""))
                    .Where(l => !l.StartsWith(";") && !String.IsNullOrEmpty(l) && Regex.IsMatch(l.Split('=').Skip(1).First(), "^[a-fA-F0-9]+$"))
                    .ToDictionary(
                        l => l.Split('=').First(),
                        l => l.Split('=').Skip(1).First().Hexlify()
                        );

                var masterKeys = _keyMaterial.Where(k => Regex.IsMatch(k.Key, "master_key_[\\d]{2}")).Select(m => Convert.ToInt32(Regex.Match(m.Key, "[\\d]{2}").Value));
                foreach (var i in masterKeys)
                {
                    if (!_keyMaterial.ContainsKey($"key_area_key_application_{i:00}"))
                        this[$"key_area_key_application_{i:00}"] = GenerateKek(this["key_area_key_application_source"], this[$"master_key_{i:00}"], this["aes_kek_generation_source"], this["aes_key_generation_source"]);
                    if (!_keyMaterial.ContainsKey($"key_area_key_ocean_{i:00}"))
                        this[$"key_area_key_ocean_{i:00}"] = GenerateKek(this["key_area_key_ocean_source"], this[$"master_key_{i:00}"], this["aes_kek_generation_source"], this["aes_key_generation_source"]);
                    if (!_keyMaterial.ContainsKey($"key_area_key_system_{i:00}"))
                        this[$"key_area_key_system_{i:00}"] = GenerateKek(this["key_area_key_system_source"], this[$"master_key_{i:00}"], this["aes_kek_generation_source"], this["aes_key_generation_source"]);

                    if (!_keyMaterial.ContainsKey($"titlekek_{i:00}"))
                        this[$"titlekek_{i:00}"] = Decryption.ECB128(this["titlekek_source"], this[$"master_key_{i:00}"]);
                }
            }

            private byte[] GenerateKek(byte[] generationSource, byte[] masterKey, byte[] aesKekGenSource, byte[] aesKeyGenSource)
            {
                var kek = Decryption.ECB128(aesKekGenSource, masterKey);
                var src_kek = Decryption.ECB128(generationSource, kek);

                if (aesKeyGenSource != null)
                {
                    return Decryption.ECB128(aesKeyGenSource, src_kek);
                }
                else
                {
                    return src_kek;
                }
            }
        }

        private static byte[] xci_sha256 = new byte[] { 0x2e, 0x36, 0xcc, 0x55, 0x15, 0x7a, 0x35, 0x10, 0x90, 0xa7, 0x3e, 0x7a, 0xe7, 0x7c, 0xf5, 0x81, 0xf6, 0x9b, 0x0b, 0x6e, 0x48, 0xfb, 0x06, 0x6c, 0x98, 0x48, 0x79, 0xa6, 0xed, 0x7d, 0x2e, 0x96 };

        public static void DecryptXCI(Stream input)
        {
            if (!input.CanWrite)
                throw new Exception("File not writeable. Is it open somewhere?");

            var keyset = new Keyset();
            using (var bw = new BinaryWriterX(input, true))
            using (var br = new BinaryReaderX(input, true))
            {
                //Decrypt XCI Header
                br.BaseStream.Position = 0x120;
                var iv = br.ReadBytes(0x10).Reverse().ToArray();

                br.BaseStream.Position = 0x190;
                var encrypted_xci_header = br.ReadBytes(0x70);

                bw.BaseStream.Position = 0x190;
                bw.Write(Decryption.CBC128(encrypted_xci_header, keyset["xci_header_key"], iv));

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
                        DecryptNCA(input, entry.entry.offset, keyset);
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
        public static void DecryptNCA(Stream input, long offset, Keyset keys = null)
        {
            var keyset = keys ?? new Keyset();

            using (var bw = new BinaryWriterX(input, true))
            using (var br = new BinaryReaderX(input, true))
            {
                br.BaseStream.Position = offset;

                //Decrypt NCA Header
                var enc_header = br.ReadBytes(0x400);
                var magic = enc_header.GetElements(0x200, 4).Aggregate("", (o, b) => o + (char)b);
                var headerPart = new byte[0x400];
                var headerPart2 = new byte[0x800];
                if (magic != "NCA2" && magic != "NCA3")
                {
                    headerPart = Decryption.XTS128(enc_header, keyset["header_key"], 0x200, true);
                    magic = headerPart.GetElements(0x200, 4).Aggregate("", (o, b) => o + (char)b);
                    bw.BaseStream.Position = offset;
                    bw.Write(headerPart);
                    if (magic == "NCA3")
                    {
                        headerPart2 = Decryption.XTS128(br.ReadBytes(0x800), keyset["header_key"], 0x200, true, 2);
                        bw.BaseStream.Position = offset + 0x400;
                        bw.Write(headerPart2);
                    }
                    else if (magic == "NCA2")
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            var buffer = Decryption.XTS128(br.ReadBytes(0x200), keyset["header_key"], 0x200, true);
                            Array.Copy(buffer, 0, headerPart2, i * 0x200, 0x200);
                            bw.BaseStream.Position -= 0x200;
                            bw.Write(buffer);
                        }
                    }
                    else
                    {
                        throw new InvalidDataException("Invalid NCA Header! Is the header key correct?");
                    }
                }
                else
                {
                    headerPart = enc_header;
                    headerPart2 = br.ReadBytes(0x800);
                }


                //Get crypto_type for master_key
                var cryptoType = (headerPart[0x220] > headerPart[0x206]) ? headerPart[0x220] : headerPart[0x206];
                if (cryptoType >= 1) cryptoType--;

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
                    var keyIndex = headerPart[0x207];
                    if (keyIndex > 2)
                        throw new InvalidDataException($"NCA KeyIndex must be 0-2. Found KeyIndex: {keyIndex}");

                    byte[] decKey = null;
                    switch (keyIndex)
                    {
                        case 0:
                            decKey = keyset[$"key_area_key_application_{cryptoType:00}"];
                            break;
                        case 1:
                            decKey = keyset[$"key_area_key_ocean_{cryptoType:00}"];
                            break;
                        case 2:
                            decKey = keyset[$"key_area_key_system_{cryptoType:00}"];
                            break;
                    }
                    dec_key_area = Decryption.ECB128(headerPart.GetElements(0x300, 0x40), decKey);
                }
                else
                {
                    //Decrypt title_key
                    var title_key = InputBox.Show("Input Titlekey:", "Decrypt NCA").Hexlify(16);
                    dec_title_key = Decryption.ECB128(title_key, keyset[$"titlekek_{cryptoType:00}"]);
                }

                //Read out section crypto
                List<SectionEntry> sectionList = new BinaryReaderX(new MemoryStream(headerPart.GetElements(0x240, 0x40))).ReadMultiple<SectionEntry>(4);
                for (int i = 0; i < 4; i++)
                {
                    if (sectionList[i].mediaOffset != 0 && sectionList[i].endMediaOffset != 0)
                    {
                        br.BaseStream.Position = offset + sectionList[i].mediaOffset * 0x200L;

                        if (hasRightsID)
                        {
                            var enc_buffer = br.ReadBytes((int)(sectionList[i].endMediaOffset * 0x200L - sectionList[i].mediaOffset * 0x200L));
                            var dec_buffer = Decryption.CTR128(enc_buffer, dec_title_key, GenerateCTR(i + 1, sectionList[i].mediaOffset * 0x200L));
                            bw.BaseStream.Position = offset + sectionList[i].mediaOffset * 0x200L;
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
                                    var dec_buffer = Decryption.XTS128(enc_buffer, dec_key_area.GetElements(0, 0x20), 0x200);
                                    bw.BaseStream.Position = offset + sectionList[i].mediaOffset * 0x200;
                                    bw.Write(dec_buffer);
                                    break;
                                case 3:
                                    //CTR
                                    enc_buffer = br.ReadBytes(sectionList[i].endMediaOffset * 0x200 - sectionList[i].mediaOffset * 0x200);
                                    var section_ctr = headerPart2.GetElements(i * 0x200 + 0x140, 8).Reverse().ToArray();
                                    dec_buffer = Decryption.CTR128(enc_buffer, dec_key_area.GetElements(0x20, 0x10), GenerateCTR(section_ctr, sectionList[i].mediaOffset * 0x200));
                                    bw.BaseStream.Position = offset + sectionList[i].mediaOffset * 0x200;
                                    bw.Write(dec_buffer);
                                    break;
                                case 4:
                                    //BKTR
                                    //stub
                                    break;
                            }

                            bw.BaseStream.Position = offset + 0x400 + i * 0x200 + 0x4;
                            bw.Write((byte)0x1);
                        }
                    }
                }
            }
        }

        private static byte[] GenerateCTR(byte[] section_ctr, long offset)
        {
            int ctr = 0;
            for (int i = 0; i < 4; i++)
                ctr |= section_ctr[i] << ((3 - i) * 8);

            return GenerateCTR(ctr, offset);
        }

        private static byte[] GenerateCTR(int section_ctr, long offset)
        {
            offset >>= 4;
            byte[] ctr = new byte[0x10];
            for (int i = 0; i < 4; i++)
            {
                ctr[0x4 - i - 1] = (byte)(section_ctr & 0xFF);
                section_ctr >>= 8;
            }
            for (int i = 0; i < 8; i++)
            {
                ctr[0x10 - i - 1] = (byte)(offset & 0xFF);
                offset >>= 8;
            }
            return ctr;
        }

        private static byte[] GenerateBktrCTR(int section_id, int ctr_val, long offset)
        {
            offset >>= 4;
            byte[] ctr = new byte[0x10];
            for (int i = 0; i < 4; i++)
            {
                ctr[0x4 - i - 1] = (byte)(section_id & 0xFF);
                section_id >>= 8;
            }
            for (uint i = 0; i < 0x8; i++)
            {
                ctr[0x10 - i - 1] = (byte)(offset & 0xFF);
                offset >>= 8;
            }
            for (uint j = 0; j < 4; j++)
            {
                ctr[0x8 - j - 1] = (byte)(ctr_val & 0xFF);
                ctr_val >>= 8;
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
