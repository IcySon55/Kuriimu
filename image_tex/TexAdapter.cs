using System;
using System.Drawing;
using System.IO;
using Kuriimu.Compression;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_tex
{
    public class TexAdapter : IImageAdapter
    {
        private FileInfo _fileInfo = null;
        private TEX _tex = null;

        #region Properties

        // Information
        public string Name => Properties.Settings.Default.PluginName;

        public string Description => "Normal Texture";

        public string Extension => "*.tex";

        public string About => "This is the TEX file adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;

        public bool CanSave => true;

        public FileInfo FileInfo
        {
            get
            {
                return _fileInfo;
            }
            set
            {
                _fileInfo = value;
            }
        }

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

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists)
                _tex = new TEX(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read));
            else
                result = LoadResult.FileNotFound;

            return result;
        }

        public SaveResult Save(string filename = "")
        {
            SaveResult result = SaveResult.Success;

            if (filename.Trim() != string.Empty)
                _fileInfo = new FileInfo(filename);

            try
            {
                _tex.Save(new FileStream(_fileInfo.FullName, FileMode.Create, FileAccess.Write));
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
                return _tex.Image;
            }
            set
            {
                _tex.Image = value;
            }
        }

        public bool ShowProperties(Icon icon) => throw new NotImplementedException();
    }
}
