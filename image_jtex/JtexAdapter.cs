using System;
using System.Drawing;
using System.IO;
using Cetera.Image;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_jtex
{
    class JtexAdapter : IImageAdapter
    {
        private FileInfo _fileInfo = null;
        private JTEX _jtex = null;
        private RawJTEX _rawjtex = null;
        private bool raw = false;

        #region Properties

        // Information
        public string Name => Properties.Settings.Default.PluginName;
        public string Description => "J Texture";
        public string Extension => "*.jtex";
        public string About => "This is the JTEX file adapter for Kukkii.";

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
                if (filename.Split('.')[filename.Split('.').Length - 1] == "jtex")
                {
                    raw = true;
                    return true;
                }
                return br.ReadString(4) == "jIMG";
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            _fileInfo = new FileInfo(filename);

            if (_fileInfo.Exists)
                if (raw)
                {
                    _rawjtex = new RawJTEX(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read));
                }
                else
                {
                    _jtex = new JTEX(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read));
                }
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
                if (raw)
                {
                    _rawjtex.Save(new FileStream(_fileInfo.FullName, FileMode.Create, FileAccess.Write));
                }
                else
                {
                    _jtex.Save(new FileStream(_fileInfo.FullName, FileMode.Create, FileAccess.Write));
                }
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
                if (raw)
                {
                    return _rawjtex.Image;
                }
                return _jtex.Image;
            }
            set
            {
                if (raw)
                {
                    _rawjtex.Image = value;
                }
                else
                {
                    _jtex.Image = value;
                }
            }
        }

        public bool ShowProperties(Icon icon) => false;
    }
}
