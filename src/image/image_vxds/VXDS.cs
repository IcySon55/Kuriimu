using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

/*Code by Gericom, ported to a plugin by onepiecefreak*/

namespace image_vxds
{
    public class VXDS
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        public VXDS(Stream input)
        {
            VxDemuxer dm = new VxDemuxer(input);
            MobiclipDecoder d = new MobiclipDecoder(dm.Header.Width, dm.Header.Height, MobiclipDecoder.MobiclipVersion.VxDS);

            while (true)
            {
                uint NrAudioPackets;
                bool IsKeyFrame;
                byte[] framedata = dm.ReadFrame(out NrAudioPackets);
                if (framedata == null) break;
                d.Data = framedata;
                d.Offset = 0;
                Bitmap b = d.DecodeFrame();

                bmps.Add(new Bitmap(b));
            }
        }

        public void Save(string filename)
        {

        }
    }
}
