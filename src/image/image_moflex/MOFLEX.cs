using System.Collections.Generic;
using System.Drawing;
using System.IO;

/*Code by Gericom, ported to a plugin by onepiecefreak*/

namespace image_moflex
{
    public class MOFLEX
    {
        public List<Bitmap> bmps = new List<Bitmap>();

        public MOFLEX(Stream input)
        {
            MobiclipDecoder decoder = null;
            var demux = new MoLiveDemux(input);
            int PlayingVideoStream = -1;

            bool first = true;

            demux.OnCompleteFrameReceived += delegate (MoLiveChunk Chunk, byte[] Data)
            {
                if ((Chunk is MoLiveStreamVideo || Chunk is MoLiveStreamVideoWithLayout) && ((PlayingVideoStream == -1) || ((MoLiveStream)Chunk).StreamIndex == PlayingVideoStream))
                {
                    if (decoder == null)
                    {
                        decoder = new MobiclipDecoder(((MoLiveStreamVideo)Chunk).Width, ((MoLiveStreamVideo)Chunk).Height, MobiclipDecoder.MobiclipVersion.Moflex3DS);
                        PlayingVideoStream = ((MoLiveStream)Chunk).StreamIndex;
                    }
                    decoder.Data = Data;
                    decoder.Offset = 0;
                    //if (first)
                    //{
                    Bitmap b = decoder.DecodeFrame();
                    bmps.Add(new Bitmap(b));
                    //}

                    first = false;
                }
            };

            bool left = false;
            int counter = 0;
            while (true)
            {
                uint error = demux.ReadPacket();
                if (error == 73)
                    break;
            }
        }

        public void Save(string filename)
        {

        }
    }
}
