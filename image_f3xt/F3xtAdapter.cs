using System;
using System.Drawing;
using System.IO;
using Kuriimu.Compression;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_f3xt
{
    public class F3xtAdapter : IImageAdapter
    {
        private F3XT _f3xt = null;

        #region Properties

        // Information
        public string Name => image_f3xt.Properties.Settings.Default.PluginName;

        public string Description => "F3XT Texture";
        public string Extension => "*.tex";
        public string About => "This is the F3XT file adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;

                //check for compression
                if (br.ReadByte() == 0x11)
                {
                    br.BaseStream.Position = 0;
                    uint size = br.ReadUInt32() >> 8;
                    br.BaseStream.Position = 0;
                    byte[] decomp = LZ11.Decompress(br.BaseStream);
                    if (decomp.Length == size)
                    {
                        if (new BinaryReaderX(new MemoryStream(decomp)).ReadString(4) == "F3XT")
                        {
                            return true;
                        }
                    }
                }
                br.BaseStream.Position = 0;

                return br.ReadString(4) == "F3XT";
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _f3xt = new F3XT(FileInfo.OpenRead());
            else
                result = LoadResult.FileNotFound;

            return result;
        }

        public SaveResult Save(string filename = "")
        {
            SaveResult result = SaveResult.Success;

            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            try
            {
                _f3xt.Save(FileInfo.OpenWrite());
            }
            catch (Exception)
            {
                result = SaveResult.Failure;
            }

            return result;
        }

        // Bitmaps
        public Bitmap Bitmap
        {
            get
            {
                return _f3xt.Image;
            }
            set
            {
                _f3xt.Image = value;
            }
        }

        public bool ShowProperties(Icon icon) => false;
    }
}
