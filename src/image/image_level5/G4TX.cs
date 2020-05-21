using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Kontract.Hash;
using Kontract.IO;

namespace image_xi.G4TX
{
    public class G4TX
    {
        public List<G4TXBitmapInfo> bmps = new List<G4TXBitmapInfo>();

        private Header _header;
        private List<TextureEntry> _textureEntries;
        private List<byte> _unkIds;

        public G4TX(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                _header = br.ReadStruct<Header>();
                _textureEntries = br.ReadMultiple<TextureEntry>(_header.textureCount);
                var hashes = br.ReadMultiple<int>(_header.textureCount);
                _unkIds = br.ReadMultiple<byte>(_header.textureCount);

                var stringOffset = br.BaseStream.Position;
                var stringOffsets = br.ReadMultiple<short>(_header.textureCount);

                for (int i = 0; i < _header.textureCount; i++)
                {
                    br.BaseStream.Position = stringOffset + stringOffsets[i];
                    var name = br.ReadCStringA();

                    var offset = (_textureEntries[i].nxtchOffset + _header.headerSize + _header.contentSize + 0xF) & ~0xF;
                    var nxtch = new NXTCH(new SubStream(input, offset, _textureEntries[i].nxtchSize));
                    bmps.Add(new G4TXBitmapInfo
                    {
                        Bitmap = nxtch.bmp,
                        Name = name,
                        TextureEntry = nxtch
                    });
                }
            }
        }

        public void Save(string filename)
        {
            using (var bw = new BinaryWriterX(File.OpenWrite(filename)))
            {
                // Update texture sizes
                for (int i = 0; i < _header.textureCount; i++)
                {
                    _textureEntries[i].width = (short)(2 << (int)Math.Log(bmps[i].Bitmap.Width - 1, 2));
                    _textureEntries[i].height = (short)(2 << (int)Math.Log(bmps[i].Bitmap.Height - 1, 2));
                }

                bw.BaseStream.Position = _header.headerSize + _header.textureCount * 0x30;

                // Write hashes
                foreach (var img in bmps)
                    bw.Write(Crc32.Create(img.Name));

                // Write unknown Ids
                foreach (var unkId in _unkIds)
                    bw.Write(unkId);
                bw.WriteAlignment(4);

                // Write string offsets
                var stringOffset = ((bw.BaseStream.Position + _header.textureCount * 2 + 0xF) & ~0xF) - bw.BaseStream.Position;
                foreach (var img in bmps)
                {
                    bw.Write((short)stringOffset);
                    stringOffset += Encoding.ASCII.GetByteCount(img.Name) + 1;
                }
                bw.WriteAlignment();

                // Write strings
                foreach (var img in bmps)
                    bw.WriteASCII(img.Name + '\0');
                _header.contentSize = (int)(bw.BaseStream.Length - _header.headerSize);
                bw.WriteAlignment(0x10, 0xaa);
                var textureDataOffset = (int)bw.BaseStream.Position;

                // Write texture datas
                for (int i = 0; i < _header.textureCount; i++)
                {
                    _textureEntries[i].nxtchOffset = (int)bw.BaseStream.Position - textureDataOffset;
                    bmps[i].TextureEntry.Save(bw.BaseStream, (int)bw.BaseStream.Position);
                    _textureEntries[i].nxtchSize = (int)bw.BaseStream.Length - _textureEntries[i].nxtchOffset - textureDataOffset;
                }

                // Write texture entries
                bw.BaseStream.Position = _header.headerSize;
                foreach (var entry in _textureEntries)
                    bw.WriteStruct(entry);

                // Write header
                _header.textureDataSize = (int)bw.BaseStream.Length - textureDataOffset;
                bw.BaseStream.Position = 0;
                bw.WriteStruct(_header);
            }
        }
    }
}
