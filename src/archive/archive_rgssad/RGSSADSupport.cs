using Kontract.Interface;
using System;

namespace archive_rgssad
{
    public static class RGSSADSupport
    {
        public static byte[] decryptFileData(byte[] data, uint key)
        {
            byte[] decrypted = new byte[data.Length];

            uint tempKey = key;
            byte[] keyBytes = BitConverter.GetBytes(key);

            int j = 0;

            for (int i = 0; i < data.Length; ++i)
            {
                if (j == 4)
                {
                    j = 0;
                    tempKey *= 7;
                    tempKey += 3;
                    keyBytes = BitConverter.GetBytes(tempKey);
                }

                decrypted[i] = (byte)(data[i] ^ keyBytes[j]);

                j++;
            }

            return decrypted;
        }
    }

    public class RGSSADFileInfo : ArchiveFileInfo
    {
        public RGSSADFileEntry Entry;
    }

    public class RGSSADFileEntry
    {
        public string Name;
        public int Size;
        public long Offset;
        public uint Key;
        public byte[] Data;
    }

    public enum RGSSADVersion
    {
        RGSSADv1 = 1,
        RGSSADv3 = 3
    }
}
