using System;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_tm2
{
    public sealed class Tm2Adapter : IImageAdapter
    {
        private FileInfo _fileInfo = null;
        private TM2 _tm2 = null;

        #region Properties

        public string Name => "TM2";
        public string Description => "Texture Matrix 2";
        public string Extension => "*.bip";
        public string About => "This is the TM2 image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

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
                int count = br.ReadInt32();
                if (br.BaseStream.Length < count * 0x4) return false;
                br.BaseStream.Position = (count - 1) * 0x4;
                int off = br.ReadInt32();
                if (br.BaseStream.Length < off+8) return false;
                br.BaseStream.Position = off;
                return (br.ReadString(8) == "EMUARC__");
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists)
                _tm2 = new TM2(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read));
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
                //_tm2.Save(_fileInfo.FullName);
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
                return _tm2.bmp;
            }
            set
            {
                _tm2.bmp = value;
            }
        }

        public bool ShowProperties(Icon icon) => false;
    }
}
