using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using archive_l7c.Compression;

namespace archive_ff1_dpk
{
    public class Wp16 : BaseLz
    {
        protected override int PreBufferLength => 0x42;

        protected override Wp16Encoder CreateEncoder()
        {
            return new Wp16Encoder();
        }

        protected override NewOptimalParser CreateParser(int inputLength)
        {
            return new NewOptimalParser(new Wp16PriceCalculator(), 0,
                new BackwardLz77MatchFinder(4, 0x42, 2, 0xFFE, true, DataType.Short));
        }

        protected override Wp16Decoder CreateDecoder()
        {
            return new Wp16Decoder();
        }

        public override string[] Names => new[] { "Wp16" };
    }
}
