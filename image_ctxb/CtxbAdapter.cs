using System;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_ctxb
{
    public sealed class CtxbAdapter : IImageAdapter
    {
        private FileInfo _fileInfo = null;
        private CTXB _ctxb = null;

        #region Properties

        public string Name => "CTXB";
        public string Description => "Whatever CTXB should mean";
        public string Extension => "*.ctxb";
        public string About => "This is the CTXB image adapter for Kukkii.";

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
                return br.ReadString(4) == "ctxb";
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists)
                _ctxb = new CTXB(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read));
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
                //_ctxb.Save(_fileInfo.FullName, _ctxb.bmp);
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
                return _ctxb.bmp;
            }
            set
            {
                _ctxb.bmp = value;
            }
        }

        public bool ShowProperties(Icon icon) => false;
    }
}
