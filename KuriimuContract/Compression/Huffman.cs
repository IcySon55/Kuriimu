using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using Kuriimu.IO;
using System.Text;
using System;

namespace Kuriimu.Compression
{
    public class Huffman
    {
        //Huffman 4bit/8bit
        public static byte[] Decompress(Stream input, int num_bits, long decompressedLength=0)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                var version = br.ReadByte();
                if (version == 0x24 || version == 0x28)
                {
                    br.BaseStream.Position--;
                    decompressedLength = br.ReadUInt32() >> 8;
                }
                else
                {
                    br.BaseStream.Position--;
                    version = 0;
                }

                var result = new List<byte>();

                var tree_size = br.ReadByte();
                var tree_root = br.ReadByte();
                var tree_buffer = br.ReadBytes(tree_size * 2);

                for (int i = 0, code = 0, next = 0, pos = tree_root; ; i++)
                {
                    if (i % 32 == 0)
                    {
                        code = br.ReadInt32();
                    }
                    next += (pos & 0x3F) * 2 + 2;
                    int direction = (code >> (31 - i)) % 2 == 0 ? 2 : 1;
                    var leaf = (pos >> 5 >> direction) % 2 != 0;

                    pos = tree_buffer[next - direction];
                    if (leaf)
                    {
                        result.Add((byte)pos);
                        pos = tree_root;
                        next = 0;
                    }

                    if (result.Count == decompressedLength * 8 / num_bits)
                    {
                        if (version == 0)
                            return num_bits == 8 ? result.ToArray() :
                                Enumerable.Range(0, (int)decompressedLength).Select(j => (byte)(result[2 * j + 1] * 16 + result[2 * j])).ToArray();
                        else
                            return num_bits == 8 ? result.ToArray() :
                                Enumerable.Range(0, (int)decompressedLength).Select(j => (byte)(result[2 * j] * 16 + result[2 * j + 1])).ToArray();

                    }
                }
            }
        }

        public static byte[] Compress(Stream input, int num_bits)
        {
            if (input.Length > 0xFFFFFF)
                throw new Exception("File too big");

            List<byte> result = new List<byte>();
            List<bool> data = new List<bool>();

            byte[] inData = new byte[input.Length];
            input.Read(inData, 0, (int)input.Length);

            //Get frequencies
            var freq = (from b in inData
                        group b by b into g
                        orderby g.Key descending
                        let pair = new Node(null, null, g.Count(), g.Key)
                        orderby pair.freqCount descending
                        select pair).ToList();

            //Sort and create the tree
            var count = 0;
            while (freq.Count() > 1)
            {
                var node = new Node(null, null, -1, -1);

                node.child0 = freq.Last();
                List<Node> tmp=new List<Node>();
                for (int i = 0; i < freq.Count() - 1; i++) tmp.Add(freq[i]);
                freq = tmp;

                node.child1 = freq.Last();
                tmp = new List<Node>();
                for (int i = 0; i < freq.Count() - 1; i++) tmp.Add(freq[i]);
                freq = tmp;

                node.freqCount = node.child0.freqCount + node.child1.freqCount;
                node.code = int.MaxValue - (int)Math.Pow(2, num_bits) + count++;
                freq.Add(node);
                freq = (from b in freq
                        orderby b.freqCount descending, b.code descending
                        select b).ToList();

                var t = freq;
            }
            var huffTree = freq[0];

            //Get HuffCodes
            Dictionary<int, List<bool>> huffCodes = new Dictionary<int, List<bool>>();
            GetHuffCodes(huffTree, huffCodes);

            //Encode HuffTree
            List<byte> encTree = new List<byte>();
            EncodeHuffTree(encTree, new List<Node> { huffTree }, (int)Math.Pow(2, num_bits));

            //Write header + tree
            result.Add((byte)(0x20 | num_bits));
            result.Add((byte)(input.Length & 0xFF));
            result.Add((byte)(input.Length >> 8 & 0xFF));
            result.Add((byte)(input.Length >> 8 & 0xFF));
            int treeSize = encTree.Count / 2;
            result.Add((byte)treeSize);
            result.AddRange(encTree);

            bool added = false;
            while (result.Count % 4 != 0) { result.Add(0); added = true; }
            if (added) treeSize++;
            result[4] = (byte)treeSize;

            //Write encoded data
            foreach (var part in inData)
            {
                data.AddRange(huffCodes[part].ToArray().Reverse().ToList());
            }
            while (data.Count % 32 != 0) data.Add(false);

            var s = data;

            count = 0;
            for (int i=0;i<data.Count;i+=32)
            {
                result.AddRange(new byte[] { 0, 0, 0, 0 });
                for (int j=0;j<4;j++)
                {
                    for (int h = 0; h < 8; h++)
                    {
                        byte bit = (data[i + j * 8 + h]) ? (byte)1 : (byte)0;
                        result[result.Count - (j + 1)] |= (byte)(bit << (7 - h));
                    }
                }
            }

            return result.ToArray();
        }

        private static void GetHuffCodes(Node tree, Dictionary<int,List<bool>> huffCodes, int length=0, int huffCode=0)
        {
            bool added = false;

            if (tree.child0 != null)
                GetHuffCodes(tree.child0, huffCodes, length + 1, huffCode << 1);
            else
            {
                List<bool> tmp = new List<bool>();
                for (int i = length-1; i >=0; i--)
                {
                    tmp.Add((huffCode % 2 == 0) ? false : true);
                    huffCode >>= 1;
                }
                huffCodes.Add(tree.code, tmp);
                if (tree.child0==null) added = true;
            }

            if (tree.child1 != null)
                GetHuffCodes(tree.child1, huffCodes, length + 1, (huffCode << 1) + 1);
            else
            {
                if (!added)
                {
                    List<bool> tmp = new List<bool>();
                    for (int i = length - 1; i >= 0; i--)
                    {
                        tmp[i] = (huffCode % 2 == 0) ? false : true;
                        huffCode >>= 1;
                    }
                    huffCodes.Add(tree.code, tmp);
                }
            }
        }

        public static void EncodeHuffTree(List<byte> encTree, List<Node> stage, int pow, byte offset = 0)
        {
                for (int i = 0; i < stage.Count; i++)
                {
                    if (stage[i] != null)
                    {
                        byte data = 0;
                        if (stage[i].child0 == null && stage[i].child1 == null)
                        {
                            encTree.Add((byte)stage[i].code);
                        }
                        else
                        {
                            if (stage[i].child0.code < pow) data |= 0x80;
                            if (stage[i].child1.code < pow) data |= 0x40;
                            data += offset;
                            encTree.Add(data);
                            if (stage.Count > 1) offset++;
                        }
                        if (i % 2 == 1) offset--;
                    }
                }

            var stageTmp = new List<Node>();
            for (int i = 0; i < stage.Count; i++)
            {
                if (stage[i] != null)
                {
                    stageTmp.Add(stage[i].child0);
                    stageTmp.Add(stage[i].child1);
                }
            }

            if (stageTmp.Count>0) EncodeHuffTree(encTree, stageTmp, pow, offset);
        }

        public class Node
        {
            public Node(Node child0, Node child1, int freqCount, int code)
            {
                this.child0 = child0;
                this.child1 = child1;
                this.freqCount = freqCount;
                this.code = code;
            }

            public Node child0;
            public Node child1;
            public int freqCount;
            public int code;
        }
    }
}
