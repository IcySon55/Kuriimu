using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Kontract.Image;
using Kontract.Image.Format;
using Kontract.Image.Swizzle;
using Kontract;
using Kontract.IO;

namespace image_nintendo.SMDH
{
    public class SMDH
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public Magic magic;
            public short version;
            public short reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class AppSettings
        {
            public AppSettings(Stream input)
            {
                using (var br = new BinaryReaderX(input, true))
                {
                    gameRating = br.ReadBytes(0x10);
                    regionLockout = br.ReadInt32();
                    makerID = br.ReadInt32();
                    makerBITID = br.ReadInt64();
                    flags = br.ReadInt32();
                    eulaVerMinor = br.ReadByte();
                    eulaVerMajor = br.ReadByte();
                    reserved = br.ReadInt16();
                    animDefaultFrame = br.ReadInt32();
                    streetPassID = br.ReadInt32();
                }
            }

            public byte[] gameRating = new byte[0x10];
            public int regionLockout;
            public int makerID;
            public long makerBITID;
            public int flags;
            public byte eulaVerMinor;
            public byte eulaVerMajor;
            public short reserved;
            public int animDefaultFrame;
            public int streetPassID;

            public void Write(Stream input)
            {
                using (var bw = new BinaryWriterX(input, true))
                {
                    bw.Write(gameRating);
                    bw.Write(regionLockout);
                    bw.Write(makerID);
                    bw.Write(makerBITID);
                    bw.Write(flags);
                    bw.Write(eulaVerMinor);
                    bw.Write(eulaVerMajor);
                    bw.Write(reserved);
                    bw.Write(animDefaultFrame);
                    bw.Write(streetPassID);
                }
            }
        }

        public Header header;
        public List<string> shortDesc = new List<string>();
        public List<string> longDesc = new List<string>();
        public List<string> publisher = new List<string>();
        public AppSettings appSettings;

        public List<Bitmap> bmps = new List<Bitmap>();

        public SMDH(Stream input)
        {
            using (var br = new BinaryReaderX(input))
            {
                //Header
                header = br.ReadStruct<Header>();

                //Application Titles
                for (int i = 0; i < 0x10; i++)
                {
                    shortDesc.Add(Encoding.GetEncoding("UTF-16").GetString(br.ReadBytes(0x80)));
                    longDesc.Add(Encoding.GetEncoding("UTF-16").GetString(br.ReadBytes(0x100)));
                    publisher.Add(Encoding.GetEncoding("UTF-16").GetString(br.ReadBytes(0x80)));
                }

                //Application Settings
                appSettings = new AppSettings(br.BaseStream);
                br.BaseStream.Position += 0x8;

                var settings = new ImageSettings
                {
                    Width = 24,
                    Height = 24,
                    Format = new RGBA(5, 6, 5),
                    Swizzle = new CTRSwizzle(24, 24)
                };
                bmps.Add(Common.Load(br.ReadBytes(0x480), settings));

                settings = new ImageSettings
                {
                    Width = 48,
                    Height = 48,
                    Format = new RGBA(5, 6, 5),
                    Swizzle = new CTRSwizzle(48, 48)
                };
                bmps.Add(Common.Load(br.ReadBytes(0x1200), settings));
            }
        }

        public void Save(Stream output)
        {
            if (bmps[0].Width != 24 || bmps[0].Height != 24) throw new System.Exception("The size of the icons can't be changed");
            if (bmps[1].Width != 48 || bmps[1].Height != 48) throw new System.Exception("The size of the icons can't be changed");

            using (BinaryWriterX bw = new BinaryWriterX(output))
            {
                //Header
                bw.WriteStruct(header);

                //Application Titles
                for (int i = 0; i < 0x10; i++)
                {
                    bw.Write(Encoding.GetEncoding("UTF-16").GetBytes(shortDesc[i]));
                    bw.Write(Encoding.GetEncoding("UTF-16").GetBytes(longDesc[i]));
                    bw.Write(Encoding.GetEncoding("UTF-16").GetBytes(publisher[i]));
                }

                //Application Settings
                appSettings.Write(bw.BaseStream);
                bw.BaseStream.Position = bw.BaseStream.Position + 0xf & ~0xf;

                //Bitmap Data
                var settings = new ImageSettings
                {
                    Width = 24,
                    Height = 24,
                    Format = new RGBA(5, 6, 5),
                    Swizzle = new CTRSwizzle(24, 24)
                };
                bw.Write(Common.Save(bmps[0], settings));

                settings = new ImageSettings
                {
                    Width = 48,
                    Height = 48,
                    Format = new RGBA(5, 6, 5),
                    Swizzle = new CTRSwizzle(48, 48)
                };
                bw.Write(Common.Save(bmps[1], settings));
            }
        }
    }
}
