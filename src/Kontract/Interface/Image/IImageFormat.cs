using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace Kuriimu.Kontract
{
    public interface IImageFormat
    {
        int bitDepth { get; set; }

        IEnumerable<Color> Load(byte[] input);
        void Save(Color color, Stream output);
    }
}
