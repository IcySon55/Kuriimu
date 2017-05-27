using System;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_nintendo.BCLYT
{
    public class BclytAdapter : IImageAdapter
    {
        private Bitmap _bclyt = null;

        #region Properties

        public string Name => "BCLYT";
        public string Description => "Standard Nintendo Layout format";
        public string Extension => "*.bclyt";
        public string About => "This is the BCLYT image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.ReadString(4) == "CLYT")
                {
                    return true;
                }
                return false;
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _bclyt = BCLYT.Load(FileInfo.OpenRead(), filename);
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
                //_bclyt.Save(_fileInfo.FullName);
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
                return _bclyt;
            }
            set
            {
                _bclyt = value;
            }
        }

        public bool ShowProperties(Icon icon) => throw new NotImplementedException();
    }
}
