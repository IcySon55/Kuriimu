using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Cetera.Image;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_nintendo.ICN
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
                    eulaVer = br.ReadInt16();
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
            public short eulaVer;
            public short reserved;
            public int animDefaultFrame;
            public int streetPassID;
        }

        public Header header;
        public List<string> shortDesc = new List<string>();
        public List<string> longDesc = new List<string>();
        public List<string> publisher = new List<string>();
        public AppSettings appSettings;

        public Bitmap bmp;
        ImageSettings settings;

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

                settings = new ImageSettings
                {
                    Width = 24,
                    Height = 24,
                    Format = Format.RGB565,
                    PadToPowerOf2 = false
                };
                bmp = Common.Load(br.ReadBytes(0x480), settings);
            }
        }

        public void Save(string filename, Bitmap bitmap)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.Create(filename)))
            {

            }
        }
    }
}
