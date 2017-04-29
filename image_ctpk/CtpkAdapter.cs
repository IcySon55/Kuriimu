using System;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_ctpk
{
    public sealed class CtpkAdapter : IImageAdapter
    {
        private FileInfo _fileInfo = null;
        private CTPK _ctpk = null;

        #region Properties

        public string Name => "CTPK";
        public string Description => "CTR Texture PaCkage";
        public string Extension => "*.ctpk";
        public string About => "This is the CTPK file adapter for Kukkii.";

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
                return br.ReadString(4) == "CTPK" || Path.GetExtension(filename) == ".ctpk";
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists)
                using (var br = new BinaryReaderX(File.OpenRead(_fileInfo.FullName)))
                    if (br.ReadString(4) == "CTPK")
                        _ctpk = new CTPK(_fileInfo.FullName);
                    else
                        _ctpk = new CTPK(_fileInfo.FullName, true);
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
                _ctpk.Save(_fileInfo.FullName);
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
                return _ctpk.bmp;
            }
            set
            {
                _ctpk.bmp = value;
            }
        }

        public bool ShowProperties(Icon icon) => throw new NotImplementedException();
    }
}
