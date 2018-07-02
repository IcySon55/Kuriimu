using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kontract.IO;
using Kontract.Hash;
using LZ4;

//Original LZ4 compliant - https://github.com/lz4/lz4
//Frame format: https://github.com/lz4/lz4/blob/dev/doc/lz4_Frame_format.md
//Block format: https://github.com/lz4/lz4/blob/dev/doc/lz4_Block_format.md

namespace Kontract.Compression
{
    public class LZ4
    {
        public static byte[] Decompress(Stream instream, bool precedingSize = false)
        {
            var ms = new MemoryStream();
            using (var bw = new BinaryWriterX(ms, true))
            using (var br2 = new BinaryReaderX(ms, true))
            using (var br = new BinaryReaderX(instream, true))
            {
                uint decompressedSize = 0;
                if (precedingSize)
                    decompressedSize = br.ReadUInt32();

                var magic = br.ReadUInt32();
                if (magic != 0x184D2204)
                    throw new Exception("LZ4 magic isn't valid.");

                var flg = br.ReadByte();
                if ((flg >> 6) != 1)
                    throw new Exception($"Unsupported Version {flg >> 6}.");

                bool blockIndep = ((flg >> 5) & 1) == 1;
                bool blockChecksum = ((flg >> 4) & 1) == 1;
                bool contentSize = ((flg >> 3) & 1) == 1;
                bool contentChecksum = ((flg >> 2) & 1) == 1;
                bool dictID = (flg & 1) == 1;

                var bd = br.ReadByte();
                BlockMaxSize blockMaxSize = (BlockMaxSize)((bd >> 4) & 0x7);

                if (contentSize)
                    br.ReadUInt64();

                if (dictID)
                    br.ReadInt32();

                var headerLength = 3 + ((contentSize) ? 8 : 0) + ((dictID) ? 4 : 0);
                br.BaseStream.Position -= headerLength - 1;
                var header = br.ReadBytes(headerLength - 1);
                var hc = br.ReadByte();
                if (((XXH32.Create(header) >> 8) & 0xFF) != hc)
                    throw new Exception("Header Checksum is invalid.");

                //Blocks
                while (br.BaseStream.Position < br.BaseStream.Length - ((contentChecksum) ? 4 : 0))
                {
                    var blockSize = br.ReadInt32();
                    while (blockSize != 0)
                    {
                        var compData = br.ReadBytes(blockSize);
                        var decomp = DecompressBlock(compData);

                        if (blockChecksum)
                        {
                            var check = br.ReadUInt32();
                            if (check != XXH32.Create(compData))
                                throw new Exception("One block checksum was invalid.");
                        }

                        bw.Write(decomp);

                        blockSize = br.ReadInt32();
                    }
                }

                if (contentChecksum)
                {
                    br2.BaseStream.Position = 0;
                    if (br.ReadUInt32() != XXH32.Create(br2.ReadAllBytes()))
                        throw new Exception("Decompressed data is corrupted.");
                }

                if (precedingSize && decompressedSize != br2.BaseStream.Length)
                    throw new Exception("Preceding decompressed size doesn't match.");
            }

            return ms.ToArray();
        }

        private static byte[] DecompressBlock(byte[] block)
        {
            List<byte> result = new List<byte>();

            var offset = 0;
            while (offset < block.Length)
            {
                var token = block[offset++];
                var count = token >> 4;
                if (count == 0xF)
                {
                    while (block[offset] == 0xFF)
                    {
                        count += 0xFF;
                        offset++;
                    }
                    count += block[offset++];
                }

                for (int i = 0; i < count; i++)
                    result.Add(block[offset++]);

                if (offset >= block.Length)
                    break;

                var off = block[offset++] | (block[offset++] << 8);
                if (off == 0)
                    throw new Exception("Offset value 0 is invalid.");

                var match = 4;
                match += token & 0xF;
                if ((token & 0xF) == 0xF)
                {
                    while (block[offset] == 0xFF)
                    {
                        match += 0xFF;
                        offset++;
                    }
                    match += block[offset++];
                }
                for (int i = 0; i < match; i++)
                    result.Add(result[result.Count - off]);
            }

            return result.ToArray();
        }

        public enum BlockMaxSize : byte
        {
            KB64 = 4,
            KB256,
            MB1,
            MB4
        }

        public static byte[] Compress(Stream instream, bool precedingSize = false, bool blockIndep = true, bool blockChecksum = false, bool contentSize = false, bool contentChecksum = true, bool dictID = false, BlockMaxSize blockMaxSize = BlockMaxSize.MB1)
        {
            var ms = new MemoryStream();
            using (var br = new BinaryReaderX(ms, true))
            using (var bw = new BinaryWriterX(ms, true))
            {
                if (precedingSize)
                    bw.Write((uint)instream.Length);

                #region Write LZ4 Frame header
                //Magic
                bw.Write(0x184D2204);

                //FLG Byte
                byte flg = 0x40;
                if (blockIndep) flg |= 0x20;
                if (blockChecksum) flg |= 0x10;
                if (contentSize) flg |= 0x08;
                if (contentChecksum) flg |= 0x04;
                if (dictID) flg |= 0x01;
                bw.Write(flg);

                //BD Byte
                byte bd = (byte)((byte)blockMaxSize << 4);
                bw.Write(bd);

                //Content Size
                if (contentSize)
                    bw.Write((long)instream.Length);

                //Dictionary - STUB since Dictionary usage isn't understood in LZ4
                if (dictID)
                    bw.Write(0);

                //XXHash32
                br.BaseStream.Position = 4;
                var fieldDesc = br.ReadBytes((int)br.BaseStream.Length - 4);
                byte hc = (byte)((XXH32.Create(fieldDesc) >> 8) & 0xFF);
                bw.Write(hc);
                #endregion

                //Write Block Data
                var comp = LZ4Codec.Encode(new BinaryReaderX(instream, true).ReadAllBytes(), 0, (int)instream.Length);
                bw.Write(comp.Length);
                bw.Write(comp);

                if (blockChecksum)
                    bw.Write(XXH32.Create(comp));

                //End Mark
                bw.Write(0);

                //Content checksum
                if (contentChecksum)
                    bw.Write(XXH32.Create(new BinaryReaderX(instream, true).ReadAllBytes()));
            }

            return ms.ToArray();
        }
    }
}
