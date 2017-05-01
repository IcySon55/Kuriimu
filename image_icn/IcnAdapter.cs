using System;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_icn
{
    public sealed class IcnAdapter : IImageAdapter
    {
        private FileInfo _fileInfo = null;
        private SMDH _icn = null;

        #region Properties

        public string Name => "SMDH Icon";
        public string Description => "SMDH";
        public string Extension => "*.icn";
        public string About => "This is the SMDH Icon file adapter for Kukkii.";

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
                return br.ReadString(4)=="SMDH";
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists)
                _icn = new SMDH(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read));
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
                return _icn.bmp;
            }
            set
            {
                _icn.bmp = value;
            }
        }

        public bool ShowProperties(Icon icon) => false;
    }
}
