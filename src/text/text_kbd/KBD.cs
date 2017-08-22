using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kuriimu.IO;

namespace text_kbd
{
    public sealed class KBD
    {
        public List<Label> Labels = new List<Label>();

        public KBD(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {

            }
        }

        public void Save(string filename)
        {
            using (var bw = new BinaryWriterX(File.OpenWrite(filename)))
            {

            }
        }
    }
}