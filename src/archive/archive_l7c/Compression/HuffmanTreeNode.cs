using System;
using System.Collections.Generic;
using System.Linq;

namespace archive_l7c.Compression
{
    public class HuffmanTreeNode
    {
        public int Code { get; set; }
        public int Frequency { get; set; }
        public HuffmanTreeNode[] Children { get; set; }

        public bool IsLeaf => Children == null;

        public HuffmanTreeNode(int frequency)
        {
            Frequency = frequency;
        }

        public IEnumerable<Tuple<int, string>> GetHuffCodes() =>
            GetHuffCodes("");

        private IEnumerable<Tuple<int, string>> GetHuffCodes(string seed) =>
            Children?.SelectMany((child, i) => child.GetHuffCodes(seed + i)) ?? new[] { new Tuple<int, string>(Code, seed) };

        public int GetDepth() => GetDepth(0);

        private int GetDepth(int seed)
        {
            if (IsLeaf || Children == null)
                return seed;

            var depth1 = Children[0].GetDepth(seed + 1);
            var depth2 = Children[1].GetDepth(seed + 1);
            return Math.Max(depth1, depth2);
        }
    }
}
