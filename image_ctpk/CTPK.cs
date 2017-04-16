using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using Cetera.Image;
using System.Linq;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_ctpk
{
    public sealed class CTPK
    {
        public Bitmap bmp;

        public CTPK(String filename, bool isRaw=false)
        {
            if (isRaw)
                GetRaw(filename);
            else
                using (BinaryReaderX br = new BinaryReaderX(File.OpenRead(filename), true))
                {
                    //Header
                    Header header = br.ReadStruct<Header>();

                    //TexEntries
                    List<Entry> entries = new List<Entry>();
                    entries.AddRange(br.ReadMultiple<Entry>(header.texCount));

                    //TexInfo List
                    List<int> texSizeList = new List<int>();
                    texSizeList.AddRange(br.ReadMultiple<int>(header.texCount));

                    //Name List
                    List<String> nameList = new List<String>();
                    for (int i = 0; i < entries.Count; i++)
                        nameList.Add(br.ReadCStringA());

                    //Hash List
                    br.BaseStream.Position = header.crc32SecOffset;
                    List<HashEntry> crc32List = new List<HashEntry>();
                    crc32List.AddRange(br.ReadMultiple<HashEntry>(header.texCount).OrderBy(e => e.entryNr));

                    //TexInfo List 2
                    br.BaseStream.Position = header.texInfoOffset;
                    List<uint> texInfoList2 = new List<uint>();
                    texInfoList2.AddRange(br.ReadMultiple<uint>(header.texCount));

                    br.BaseStream.Position = entries[0].texOffset + header.texSecOffset;
                    var settings = new ImageSettings
                    {
                        Width = entries[0].width,
                        Height = entries[0].height,
                        Format = ImageSettings.ConvertFormat(entries[0].imageFormat),
                    };
                    bmp = Common.Load(br.ReadBytes(entries[0].texDataSize), settings);
                }
        }

        public int size;

        public void GetRaw(String filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                int pixelCount = (int)br.BaseStream.Length / 2;

                int count = 1;
                bool found = false;
                while (!found && count*count<br.BaseStream.Length)
                    if (pixelCount / count == count)
                        found = true;
                    else
                        count++;

                if (found)
                {
                    size = count;
                    var settings = new ImageSettings
                    {
                        Width = size,
                        Height = size,
                        Format = Format.RGB565,
                        PadToPowerOf2 = false
                    };
                    bmp = Common.Load(br.ReadBytes((int) br.BaseStream.Length), settings);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        /*Save not supported until Bitmap[] is introduced*/

        /*public void Save(String filename)
        {
            using (BinaryWriterX bw = new BinaryWriterX(File.OpenWrite(filename)))
            {
                
            }
        }*/
    }
}
