using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Image;
using Kontract.Image.Swizzle;
using Kontract.IO;
using System;

namespace image_nintendo.CTPK
{
    public sealed class CTPK
    {
        public Header header;
        public List<CtpkEntry> entries;

        public List<Bitmap> bmps = new List<Bitmap>();
        public List<ImageSettings> _settings = new List<ImageSettings>();

        public CTPK(Stream input)
        {
            using (BinaryReaderX br = new BinaryReaderX(input))
            {
                //Header
                header = br.ReadStruct<Header>();
                entries = new List<CtpkEntry>();
                for (int i = 0; i < header.texCount; i++) entries.Add(new CtpkEntry());

                //TexEntry List
                br.BaseStream.Position = 0x20;
                foreach (var entry in entries) entry.texEntry = br.ReadStruct<TexEntry>();

                //DataSize List
                foreach (var entry in entries) for (int i = 0; i < entry.texEntry.mipLvl; i++) entry.dataSizes.Add(br.ReadUInt32());

                //Name List
                foreach (var entry in entries) entry.name = br.ReadCStringA();

                //Hash List
                br.BaseStream.Position = header.crc32SecOffset;
                List<HashEntry> hashList = br.ReadMultiple<HashEntry>(header.texCount).OrderBy(e => e.id).ToList();
                var count = 0;
                foreach (var entry in entries) entry.hash = hashList[count++];

                //MipMapInfo List
                br.BaseStream.Position = header.texInfoOffset;
                foreach (var entry in entries) entry.mipmapEntry = br.ReadStruct<MipmapEntry>();

                //Add bmps
                br.BaseStream.Position = header.texSecOffset;
                for (int i = 0; i < entries.Count; i++)
                {
                    //Main texture
                    br.BaseStream.Position = entries[i].texEntry.texOffset + header.texSecOffset;
                    var settings = new ImageSettings
                    {
                        Width = entries[i].texEntry.width,
                        Height = entries[i].texEntry.height,
                        Format = Support.CTRFormat[entries[i].texEntry.imageFormat],
                        Swizzle = new CTRSwizzle(entries[i].texEntry.width, entries[i].texEntry.height)
                    };

                    _settings.Add(settings);
                    bmps.Add(Common.Load(br.ReadBytes((int)entries[i].dataSizes[0]), settings));

                    //Mipmaps
                    if (entries[i].texEntry.mipLvl > 1)
                    {
                        for (int j = 1; j < entries[i].texEntry.mipLvl; j++)
                        {
                            settings = new ImageSettings
                            {
                                Width = settings.Width >> 1,
                                Height = settings.Height >> 1,
                                Format = Support.CTRFormat[entries[i].mipmapEntry.mipmapFormat],
                                Swizzle = new CTRSwizzle(settings.Width >> 1, settings.Height >> 1)
                            };

                            _settings.Add(settings);
                            bmps.Add(Common.Load(br.ReadBytes((int)entries[i].dataSizes[j]), settings));
                        }
                    }
                }
            }
        }

        public void Save(Stream input, bool leaveOpen)
        {
            using (BinaryWriterX bw = new BinaryWriterX(input, leaveOpen))
            {
                //check original sizes
                CheckOriginalSizes();

                //Write CTPK Header
                bw.WriteStruct(header);
                bw.BaseStream.Position = 0x20;

                //Write TexEntries
                foreach (var entry in entries) bw.WriteStruct(entry.texEntry);

                //Write dataSizes
                foreach (var entry in entries)
                    foreach (var size in entry.dataSizes) bw.Write(size);

                //Write names
                foreach (var entry in entries) bw.WriteASCII(entry.name + "\0");

                //Write hashes
                bw.BaseStream.Position = (bw.BaseStream.Position + 0x3) & ~0x3;
                List<HashEntry> hash = entries.Select(c => c.hash).OrderBy(c => c.crc32).ToList();
                foreach (var entry in hash) bw.WriteStruct(entry);

                //Write mipmapInfo
                foreach (var entry in entries) bw.WriteStruct(entry.mipmapEntry);

                //Write bitmaps
                bw.BaseStream.Position = header.texSecOffset;
                var index = 0;
                foreach (var entry in entries)
                {
                    var settings = new ImageSettings
                    {
                        Width = bmps[index].Width,
                        Height = bmps[index].Height,
                        Format = Support.CTRFormat[entry.texEntry.imageFormat],
                        Swizzle = new CTRSwizzle(bmps[index].Width, bmps[index].Height)
                    };
                    bw.Write(Common.Save(bmps[index++], settings));

                    if (entry.texEntry.mipLvl > 1)
                    {
                        for (int i = 1; i < entry.texEntry.mipLvl; i++)
                        {
                            settings = new ImageSettings
                            {
                                Width = bmps[index].Width << i,
                                Height = bmps[index].Height << i,
                                Format = Support.CTRFormat[entry.mipmapEntry.mipmapFormat],
                                Swizzle = new CTRSwizzle(bmps[index].Width << i, bmps[index].Height << i)
                            };
                            bw.Write(Common.Save(bmps[index++], settings));
                        }
                    }
                }
            }
        }

        public void CheckOriginalSizes()
        {
            var index = 0;
            foreach (var entry in entries)
            {
                var width = entry.texEntry.width;
                var height = entry.texEntry.height;
                if (bmps[index].Width != width || bmps[index].Height != height)
                    throw new Exception($"Image {index:00} has to be {width}x{height}px!");
                index++;
                if (entry.texEntry.mipLvl > 1)
                {
                    for (int i = 1; i < entry.texEntry.mipLvl; i++)
                    {
                        width >>= 1;
                        height >>= 1;
                        if (bmps[index].Width != width || bmps[index].Height != height)
                            throw new Exception($"Image {index:00} (Mipmap) has to be {width}x{height}px!");
                        index++;
                    }
                }
            }
        }
    }
}
