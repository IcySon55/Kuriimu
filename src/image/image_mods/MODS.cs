using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System;

/*Code by Gericom, ported to a plugin by onepiecefreak*/

namespace image_mods
{
    public class MODS
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        public MODS(Stream input)
        {
            ModsDemuxer demuxer = new ModsDemuxer(input);
            ModsDecoder decoder = new ModsDecoder(demuxer.Header.Width, demuxer.Header.Height);

            while (true)
            {
                uint NrAudioPackets;
                bool IsKeyFrame;
                byte[] framedata = demuxer.ReadFrame(out NrAudioPackets, out IsKeyFrame);
                if (framedata == null) break;
                decoder.Data = framedata;
                decoder.Offset = 0;
                Bitmap b = decoder.DecodeFrame();

                bmps.Add(new Bitmap(b));
            }
        }

        public void Save(string filename)
        {

        }
    }
}
