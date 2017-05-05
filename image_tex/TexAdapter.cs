using System;
using System.Drawing;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace image_tex
{
    public class TexAdapter : IImageAdapter
    {
        private TEX _tex = null;

        #region Properties

        // Information
        public string Name => Properties.Settings.Default.PluginName;
        public string Description => "MT Framework Texture";
        public string Extension => "*.tex";
        public string About => "This is the MT Framework TEX file adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return br.ReadString(3) == "TEX";
            }
        }

        public LoadResult Load(string filename)
        {
            LoadResult result = LoadResult.Success;

            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                //_tex = new TEX(FileInfo.OpenRead());
                result = LoadResult.Failure;
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
                //_tex.Save(FileInfo.OpenWrite());
                result = SaveResult.Failure;
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

        public bool ShowProperties(Icon icon) => false;
    }
}
