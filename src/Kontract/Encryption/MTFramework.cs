using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Kuriimu.IO;
using Kuriimu.Kontract;

/*Dual Destinies Android:
    key1: imaguy_uyrag_igurustim_
    key2: enokok_ikorodo_odohuran*/

namespace Kuriimu.Encryption
{
    public class MTFramework
    {
        public static byte[] Decrypt(Stream input, String key1, String key2)
        {
            using (var br = new BinaryReaderX(input))
            using (var bw = new BinaryWriterX(new MemoryStream()))
            {
                var bf = new BlowFish(GetCipherKey(key1, key2));

                //Header
                var header = br.ReadStruct<Header>();
                bw.WriteStruct(header);

                //Decrypt entries
                var entries = new List<byte[]>();
                for (int i = 0; i < header.entryCount; i++)
                {
                    var entry = ReverseByteArray(bf.Decrypt_ECB(ReverseByteArray(br.ReadBytes(0x50))));
                    entries.Add(entry);
                    bw.Write(entry);
                }

                //Decrypt archive data
                var dataOffset = 0;
                using (var entryR = new BinaryReaderX(new MemoryStream(entries[0])))
                {
                    entryR.BaseStream.Position = 0x4c;
                    dataOffset = entryR.ReadInt32();
                }
                br.BaseStream.Position = dataOffset;
                bw.BaseStream.Position = dataOffset;
                bw.Write(ReverseByteArray(bf.Decrypt_ECB(ReverseByteArray(br.ReadBytes((int)br.BaseStream.Length - dataOffset)))));

                return new BinaryReaderX(bw.BaseStream).ReadAllBytes();
            }
        }

        public static byte[] Encrypt(Stream input, String key1, String key2)
        {
            using (var br = new BinaryReaderX(input))
            using (var bw = new BinaryWriterX(new MemoryStream()))
            {
                var bf = new BlowFish(GetCipherKey(key1, key2));

                //Header
                var header = br.ReadStruct<Header>();
                bw.WriteStruct(header);

                //Decrypt entries
                var entries = new List<byte[]>();
                for (int i = 0; i < header.entryCount; i++)
                {
                    var entry = br.ReadBytes(0x50);
                    entries.Add(entry);
                    bw.Write(ReverseByteArray(bf.Encrypt_ECB(ReverseByteArray(entry))));
                }

                //Decrypt archive data
                var dataOffset = 0;
                using (var entryR = new BinaryReaderX(new MemoryStream(entries[0])))
                {
                    entryR.BaseStream.Position = 0x4c;
                    dataOffset = entryR.ReadInt32();
                }
                br.BaseStream.Position = dataOffset;
                bw.BaseStream.Position = dataOffset;
                bw.Write(ReverseByteArray(bf.Encrypt_ECB(ReverseByteArray(br.ReadBytes((int)br.BaseStream.Length - dataOffset)))));

                return new BinaryReaderX(bw.BaseStream).ReadAllBytes();
            }
        }

        static byte[] GetCipherKey(String key1, String key2)
        {
            var count = 0;
            return key2.SelectMany(c => new[] { (byte)((c ^ key1[key1.Length - count - 1]) | count++ << 6) }).ToArray();
        }

        static byte[] ReverseByteArray(byte[] input)
        {
            var result = new byte[input.Length];
            for (int i = 0; i < input.Length; i += 4)
            {
                result[i] = input[i + 3];
                result[i + 1] = input[i + 2];
                result[i + 2] = input[i + 1];
                result[i + 3] = input[i];
            }
            return result;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Header
        {
            public Magic magic;
            public short version;
            public short entryCount;
        }
    }
}
