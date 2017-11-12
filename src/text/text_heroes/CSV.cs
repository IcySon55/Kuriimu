using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kontract.IO;

namespace text_heroes
{
    public sealed class CSV
    {
        public List<Entry> Entries = new List<Entry>();

        public CSV(Stream input)
        {
            using (var br = new BinaryReaderX(input, false))
            {
               
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output, false))
            {

            }
        }
    }
}