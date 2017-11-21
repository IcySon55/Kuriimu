using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Komponent.IO;

/*Dual Destinies Android:
    key1: imaguy_uyrag_igurustim_
    key2: enokok_ikorodo_odohuran*/

namespace Komponent.Encryption
{
    public class MTFramework
    {
        public static byte[] Decrypt(Stream input, String key1, String key2)
        {
            using (var br = new BinaryReaderX(input))
            using (var bw = new BinaryWriterX(new MemoryStream()))
            {
                var bf = new BlowFish(GetCipherKey(key1, key2));
                bf.MTMethod = true;

                //Header
                var header = br.ReadStruct<Header>();
                bw.WriteStruct(header);

                //Decrypt entries
                var entries = new List<byte[]>();
                for (int i = 0; i < header.entryCount; i++)
                {
                    var entry = bf.Decrypt_ECB(br.ReadBytes(0x50));
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
                bw.Write(bf.Decrypt_ECB(br.ReadBytes((int)br.BaseStream.Length - dataOffset)));

                return new BinaryReaderX(bw.BaseStream).ReadAllBytes();
            }
        }

        public static byte[] Encrypt(Stream input, String key1, String key2)
        {
            using (var br = new BinaryReaderX(input))
            using (var bw = new BinaryWriterX(new MemoryStream()))
            {
                var bf = new BlowFish(GetCipherKey(key1, key2));
                bf.MTMethod = true;

                //Header
                var header = br.ReadStruct<Header>();
                bw.WriteStruct(header);

                //Decrypt entries
                var entries = new List<byte[]>();
                for (int i = 0; i < header.entryCount; i++)
                {
                    var entry = br.ReadBytes(0x50);
                    entries.Add(entry);
                    bw.Write(bf.Encrypt_ECB(entry));
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
                bw.Write(bf.Encrypt_ECB(br.ReadBytes((int)br.BaseStream.Length - dataOffset)));

                return new BinaryReaderX(bw.BaseStream).ReadAllBytes();
            }
        }

        static byte[] GetCipherKey(String key1, String key2) => key1.Reverse().Select((c, i) => (byte)(c ^ key2[i] | i << 6)).ToArray();

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class Header
        {
            public Magic magic;
            public short version;
            public short entryCount;
        }
    }
}
