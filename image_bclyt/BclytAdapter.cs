using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KuriimuContract;

namespace image_bclyt
{
    public class BclytAdapter : IImageAdapter
    {
        private FileInfo _fileInfo = null;
        private Bitmap _bclyt = null;

        public string Name => "BCLYT";
        public string Description => "Standard Nintendo Layout format";
        public string Extension => "*.bclyt";
        public string About => "This is the BCLYT file adapter for Kukkii.";

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

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists)
                _bclyt = BCLYT.Load(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read),filename);
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
                //_msbt.Save(_fileInfo.FullName);
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
    }
}
