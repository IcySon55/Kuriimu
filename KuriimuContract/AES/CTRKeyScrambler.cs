using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Kuriimu.CTR
{
    public class KeyScrambler
    {
        // I think at this point we can include these constants in public repositories.
        public static readonly byte[] SecretConstant =
        {
            0x1F, 0xF9, 0xE9, 0xAA, 0xC5, 0xFE, 0x04, 0x08, 0x02, 0x45, 0x91, 0xDC, 0x5D, 0x52, 0x76, 0x8A
        };

        public static readonly byte[] DSiConstant =
        {
            0xFF, 0xFE, 0xFB, 0x4E, 0x29, 0x59, 0x02, 0x58, 0x2A, 0x68, 0x0F, 0x5F, 0x1A, 0x4F, 0x3E, 0x79
        };

        public static byte[] GetNormalKey(byte[] KeyX, byte[] KeyY)
        {
            if (KeyX.Length != 0x10 || KeyY.Length != 0x10)
                throw new ArgumentException("Invalid Key Length");

            BigInteger Key = ToUnsignedBigInteger(XOR(RotateLeft(KeyX, 2), KeyY));
            BigInteger C = ToUnsignedBigInteger(SecretConstant);

            byte[] NormalKey = BigInteger.Add(Key, C).ToByteArray();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(NormalKey);

            if (NormalKey.Length > 0x10)
                NormalKey = NormalKey.Skip(1).Take(0x10).ToArray();

            return RotateRight(NormalKey, 41);
        }

        public static byte[] GetDSINormalKey(byte[] KeyX, byte[] KeyY)
        {
            if (KeyX.Length != 0x10 || KeyY.Length != 0x10)
                throw new ArgumentException("Invalid Key Length");

            BigInteger Key = ToUnsignedBigInteger(XOR(KeyX, KeyY));
            BigInteger C = ToUnsignedBigInteger(DSiConstant);

            byte[] NormalKey = BigInteger.Add(Key, C).ToByteArray();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(NormalKey);

            if (NormalKey.Length > 0x10)
                NormalKey = NormalKey.Skip(1).Take(0x10).ToArray();

            return RotateLeft(NormalKey, 42);
        }

        private static byte[] XOR(byte[] arr1, byte[] arr2)
        {
            if (arr1.Length != arr2.Length)
                throw new ArgumentException("Array Lengths must be equal.");
            byte[] xored = new byte[arr1.Length];
            for (int i = 0; i < arr1.Length; i++)
            {
                xored[i] = (byte)(arr1[i] ^ arr2[i]);
            }
            return xored;
        }

        private static BigInteger ToUnsignedBigInteger(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
                return new BigInteger(data.Reverse().Concat(new byte[] { 0 }).ToArray());
            else
                return new BigInteger(new byte[] { 0 }.Concat(data).ToArray());
        }

        private static byte[] RotateLeft(byte[] input, int shift)
        {
            int N = (input.Length * 8) - (shift % (input.Length * 8));
            List<int> bits = new List<int>();
            byte[] output = new byte[input.Length];
            foreach (byte b in input.Reverse())
            {
                for (int i = 0; i < 8; i++)
                {
                    bits.Add((b >> i) & 1);
                }
            }
            bits = bits.Skip(N).Concat(bits.Take(N)).ToList();
            for (int i = 0; i < output.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    output[i] |= (byte)(bits[i * 8 + j] << j);
                }
            }
            output = output.Reverse().ToArray();
            return output;
        }

        private static byte[] RotateRight(byte[] input, int shift)
        {
            int N = shift % (input.Length * 8);
            List<int> bits = new List<int>();
            byte[] output = new byte[input.Length];
            foreach (byte b in input.Reverse())
            {
                for (int i = 0; i < 8; i++)
                {
                    bits.Add((b >> i) & 1);
                }
            }
            bits = bits.Skip(N).Concat(bits.Take(N)).ToList();
            for (int i = 0; i < output.Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    output[i] |= (byte)(bits[i * 8 + j] << j);
                }
            }
            output = output.Reverse().ToArray();
            return output;
        }
    }
}
