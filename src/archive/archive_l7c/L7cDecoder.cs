using System;
using System.IO;
using Kontract.IO;

namespace archive_l7c
{
    public class L7cDecoder
    {
        private int _mode;

        private byte[] _windowBuffer;
        private int _windowBufferOffset;

        public L7cDecoder(int mode)
        {
            if (mode != 0x80 && mode != 0x81)
                throw new InvalidOperationException($"Unknown compression mode 0x{mode:X2}.");
            _mode = mode;
        }

        public void Decode(Stream input, Stream output)
        {
            _windowBuffer = new byte[0x8000];
            _windowBufferOffset = 0;

            switch (_mode)
            {
                case 0x80:
                    ReadMode0x80(input, output);
                    break;
                case 0x81:
                    ReadMode0x81(input, output);
                    break;
            }
        }

        // Function pointer in Tales of Hearts Infinity Evolved is 0x81005ce4

        #region Mode 0x80

        private void ReadMode0x80(Stream input, Stream output)
        {
            while (input.Position < input.Length)
            {
                var code = input.ReadByte();
                switch (code >> 6)
                {
                    case 0:
                        ReadUncompressedData(input, output, code);
                        break;
                    case 1:
                        ReadOneCompressedData(output, code);
                        break;
                    case 2:
                        ReadTwoCompressedData(input, output, code);
                        break;
                    case 3:
                        ReadThreeCompressedData(input, output, code);
                        break;
                }
            }
        }

        private void ReadUncompressedData(Stream input, Stream output, int code)
        {
            var length = code & 0x3F;
            if (code == 0)
            {
                length = 0x40;

                var byte1 = input.ReadByte();
                if (byte1 >> 7 == 0)
                {
                    length = 0xbf;

                    var byte2 = input.ReadByte();
                    length += (byte1 << 8) | byte2;
                }
                else
                {
                    length += byte1 & 0x7F;
                }
            }

            for (var i = 0; i < length; i++)
            {
                var next = (byte)input.ReadByte();
                output.WriteByte(next);
                _windowBuffer[_windowBufferOffset] = next;
                _windowBufferOffset = (_windowBufferOffset + 1) % _windowBuffer.Length;
            }
        }

        private void ReadOneCompressedData(Stream output, int code)
        {
            // 8 bits
            // 11 11 1111

            var length = ((code >> 4) & 0x3) + 2;
            var displacement = (code & 0xF) + 1;

            ReadDisplacedData(output, displacement, length);
        }

        private void ReadTwoCompressedData(Stream input, Stream output, int code)
        {
            // 16 bits
            // 11 1111 1111111111

            var byte1 = input.ReadByte();

            var length = ((code >> 2) & 0xF) + 3;
            var displacement = (((code & 0x3) << 8) | byte1) + 1;

            ReadDisplacedData(output, displacement, length);
        }

        private void ReadThreeCompressedData(Stream input, Stream output, int code)
        {
            // 24 bits
            // 11 1111111 111111111111111

            var byte1 = input.ReadByte();
            var byte2 = input.ReadByte();

            var length = (((code & 0x3F) << 1) | (byte1 >> 7)) + 4;
            var displacement = (((byte1 & 0x7F) << 8) | byte2) + 1;

            ReadDisplacedData(output, displacement, length);
        }

        private void ReadDisplacedData(Stream output, int displacement, int length)
        {
            var bufferIndex = _windowBufferOffset + _windowBuffer.Length - displacement;
            for (var i = 0; i < length; i++)
            {
                var next = _windowBuffer[bufferIndex++ % _windowBuffer.Length];
                output.WriteByte(next);
                _windowBuffer[_windowBufferOffset] = next;
                _windowBufferOffset = (_windowBufferOffset + 1) % _windowBuffer.Length;
            }
        }

        #endregion

        #region Mode 0x81

        class Node
        {
            public Node[] Children { get; } = new Node[2];
            public int Value { get; set; } = -1;
            public bool IsLeaf => Value != -1;
        }

        class Tree
        {
            private Node _root;

            public void Build(BitReader br, int valueBitCount)
            {
                _root = new Node();

                ReadNode(br, _root, valueBitCount);
            }

            public int ReadValue(BitReader br)
            {
                var node = _root;
                while (!node.IsLeaf)
                    node = node.Children[br.ReadBit()];
                return node.Value;
            }

            private void ReadNode(BitReader br, Node node, int valueBitCount)
            {
                var flag = br.ReadBit();
                if (flag != 0)
                {
                    node.Children[0] = new Node();
                    ReadNode(br, node.Children[0], valueBitCount);

                    node.Children[1] = new Node();
                    ReadNode(br, node.Children[1], valueBitCount);
                }
                else
                {
                    node.Value = br.ReadBits<int>(valueBitCount);
                }
            }
        }

        private static int[] _counters =
        {
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 0xa, 0xc, 0xe,
            0x10, 0x12, 0x16, 0x1a,
            0x1e, 0x22, 0x2a, 0x32,
            0x3a, 0x42, 0x52, 0x62,
            0x72, 0x82, 0xa2, 0xc2,
            0xe2, 0x102, 0, 0
        };

        private static int[] _counterBitReads =
        {
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 1, 1, 1,
            1, 2, 2, 2,
            2, 3, 3, 3,
            3, 4, 4, 4,
            4, 5, 5, 5,
            5, 0, 0, 0
        };

        private static int[] _dispRanges =
        {
            1, 2, 3, 4,
            5, 7, 9, 0xd,
            0x11, 0x19, 0x21, 0x31,
            0x41, 0x61, 0x81, 0xc1,
            0x101, 0x181, 0x201, 0x301,
            0x401, 0x601, 0x801, 0xc01,
            0x1001, 0x1801, 0x2001, 0x3001,
            0x4001, 0x6001, 0, 0
        };

        private static int[] _dispBitReads =
        {
            0, 0, 0, 0,
            1, 1, 2, 2,
            3, 3, 4, 4,
            5, 5, 6, 6,
            7, 7, 8, 8,
            9, 9, 0xa, 0xa,
            0xb, 0xb, 0xc, 0xc,
            0xd, 0xd, 0, 0
        };

        private void ReadMode0x81(Stream input, Stream output)
        {
            using (var br = new BitReader(input, BitOrder.LSBFirst, 1, ByteOrder.LittleEndian))
            {
                var initialByte = br.ReadBits<int>(8);

                // 3 init holders
                var rawValueMapping = new Tree();
                rawValueMapping.Build(br, 8);
                var indexValueMapping = new Tree();
                indexValueMapping.Build(br, 6);
                var dispIndexMapping = new Tree();
                dispIndexMapping.Build(br, 5);

                while (true)
                {
                    var index = indexValueMapping.ReadValue(br);

                    if (index == 0)
                    {
                        if (initialByte < 3)
                            break;

                        var iVar4 = initialByte - 2;
                        if (output.Length <= iVar4)
                            break;

                        var length = output.Length - iVar4;
                        var position = 0;
                        do
                        {
                            length--;

                            output.Position = position;
                            var byte1 = output.ReadByte();

                            output.Position = position + iVar4;
                            var byte2 = output.ReadByte();

                            output.Position--;
                            output.WriteByte((byte)(byte1 + byte2));

                            position++;
                        } while (length != 0);

                        break;
                    }

                    if (index < 0x20)
                    {
                        // Match reading
                        // Max displacement 0x8000; Min displacement 1
                        // Max length 0x102; Min length 1
                        var counter = _counters[index];
                        if (_counterBitReads[index] != 0)
                            counter += br.ReadBits<int>(_counterBitReads[index]);

                        var dispIndex = dispIndexMapping.ReadValue(br);

                        var displacement = _dispRanges[dispIndex];
                        if (_dispBitReads[dispIndex] != 0)
                            displacement += br.ReadBits<int>(_dispBitReads[dispIndex]);

                        if (counter == 0)
                            continue;

                        var bufferIndex = _windowBufferOffset + _windowBuffer.Length - displacement;
                        for (int i = 0; i < counter; i++)
                        {
                            var next = _windowBuffer[bufferIndex++ % _windowBuffer.Length];
                            output.WriteByte(next);
                            _windowBuffer[_windowBufferOffset] = next;
                            _windowBufferOffset = (_windowBufferOffset + 1) % _windowBuffer.Length;
                        }
                    }
                    else
                    {
                        // Raw data reading
                        index -= 0x20;

                        var counter = _counters[index];
                        if (_counterBitReads[index] != 0)
                            counter += br.ReadBits<int>(_counterBitReads[index]);

                        if (counter == 0)
                            continue;

                        for (int i = 0; i < counter; i++)
                        {
                            var rawValue = (byte)rawValueMapping.ReadValue(br);

                            output.WriteByte(rawValue);
                            _windowBuffer[_windowBufferOffset] = rawValue;
                            _windowBufferOffset = (_windowBufferOffset + 1) % _windowBuffer.Length;
                        }
                    }
                }
            }
        }

        #endregion

        public void Dispose()
        {
            Array.Clear(_windowBuffer, 0, _windowBuffer.Length);
            _windowBuffer = null;
        }
    }
}
