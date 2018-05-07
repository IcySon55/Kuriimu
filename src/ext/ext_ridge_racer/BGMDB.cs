using System.IO;
using System.Runtime.InteropServices;
using Kontract.IO;

namespace ext_ridge_racer
{
    public class BGMDB
    {
        private const string nIDString = "nID";
        private const string pstrBgmNameString = "pstrBgmName";
        private const string pstrArtistNameString = "pstrArtistName";
        private const string pstrFileNameString = "pstrFileName";
        private const string nOrderString = "nOrder";
        private const string nMixString = "nMix";
        private const string pstrRcidString = "pstrRcid";

        private int Magic = 0x07010200;
        private int Count = 0x1;
        private int nIDOffset = 0x60;

        private byte[] Unk1 = { 0x20, 00, 00, 00, 00, 00, 00, 00, 0x4, 00, 00, 00, 0x8, 00, 00, 00, 0xC, 00, 00, 00, 0x10, 00, 00, 00, 0x14, 00, 00, 00, 0x18, 00, 00, 00 };

        private int nIDStringOffset = 0x80;
        private int pstrBgmNameOffset = 0x84;
        private int pstrArtistNameOffset = 0x90;
        private int pstrFileNameOffset = 0x9F;
        private int nOrderOffset = 0xAC;
        private int nMixOffset = 0xB3;
        private int pstrRcidOffset = 0xB8;

        private int Unk2 = 0x05050503;
        private int Unk3 = 0x77050303;
        //public int Padding; // 0x10 (77777777777777777777777777777777)

        public DataBlock Data;
        //public int Padding; // 0x4 (77777777)

        // User Editable
        public string BgmName;
        public string ArtistName;
        public string FileName;
        public string Rcid;

        public BGMDB()
        {
            Data = new DataBlock();
            BgmName = string.Empty;
            ArtistName = string.Empty;
            FileName = "bgm##.nub3";
            Rcid = "BGM##";
        }

        public static BGMDB Load(Stream input)
        {
            var bgmdb = new BGMDB();

            using (var br = new BinaryReaderX(input))
            {
                br.BaseStream.Position = bgmdb.nIDOffset;

                // Values
                bgmdb.Data = br.ReadStruct<DataBlock>();
                bgmdb.Data.nID += 1;

                // BgmName
                br.BaseStream.Position = bgmdb.Data.pstrBgmNameOffset;
                bgmdb.BgmName = br.ReadASCIIStringUntil(0);

                // ArtistName
                br.BaseStream.Position = bgmdb.Data.pstrArtistNameOffset;
                bgmdb.ArtistName = br.ReadASCIIStringUntil(0);

                // FileName
                br.BaseStream.Position = bgmdb.Data.pstrFileNameOffset;
                bgmdb.FileName = br.ReadASCIIStringUntil(0);

                // Rcid
                br.BaseStream.Position = bgmdb.Data.pstrRcidOffset;
                bgmdb.Rcid = br.ReadASCIIStringUntil(0);
            }

            return bgmdb;
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
                // Header
                bw.Write(Magic);
                bw.Write(Count);
                bw.Write(nIDOffset);
                bw.Write(Unk1);
                bw.Write(nIDStringOffset);
                bw.Write(pstrBgmNameOffset);
                bw.Write(pstrArtistNameOffset);
                bw.Write(pstrFileNameOffset);
                bw.Write(nOrderOffset);
                bw.Write(nMixOffset);
                bw.Write(pstrRcidOffset);
                bw.Write(Unk2);
                bw.Write(Unk3);
                bw.WritePadding(0x10, 0x77);

                // Constant Strings
                bw.BaseStream.Position = nIDStringOffset;
                bw.WriteASCII(nIDString);
                bw.Write((byte)0);
                bw.WriteASCII(pstrBgmNameString);
                bw.Write((byte)0);
                bw.WriteASCII(pstrArtistNameString);
                bw.Write((byte)0);
                bw.WriteASCII(pstrFileNameString);
                bw.Write((byte)0);
                bw.WriteASCII(nOrderString);
                bw.Write((byte)0);
                bw.WriteASCII(nMixString);
                bw.Write((byte)0);
                bw.WriteASCII(pstrRcidString);
                bw.Write((byte)0);

                // BgmName
                Data.pstrBgmNameOffset = (int)bw.BaseStream.Position;
                bw.WriteASCII(BgmName);
                bw.Write((byte)0);

                // ArtistName
                Data.pstrArtistNameOffset = (int)bw.BaseStream.Position;
                bw.WriteASCII(ArtistName);
                bw.Write((byte)0);

                // FileName
                Data.pstrFileNameOffset = (int)bw.BaseStream.Position;
                bw.WriteASCII(FileName);
                bw.Write((byte)0);

                // Rcid
                Data.pstrRcidOffset = (int)bw.BaseStream.Position;
                bw.WriteASCII(Rcid);
                bw.Write((byte)0);

                // Data
                bw.BaseStream.Position = nIDOffset;
                Data.nID -= 1;
                bw.WriteStruct(Data);
                bw.WritePadding(0x4, 0x77);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class DataBlock
        {
            public int nID = 1; // User Editable
            public int pstrBgmNameOffset;
            public int pstrArtistNameOffset;
            public int pstrFileNameOffset; // bgm##.nub3
            public int nOrder = 0; // User Editable - Track position in the list
            public int nMix = 0;
            public int pstrRcidOffset; // BGM##
        }
    }
}
