using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace image_nintendo.BCLYT
{
    [FilePluginMetadata(Name = "BCLYT", Description = "Standard Nintendo Layout format", Extension = "*.bclyt",
        Author = "onepiecefreak", About = "This is the BCLYT image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public class BclytAdapter : IImageAdapter
    {
        private Bitmap _bclyt = null;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public Identification Identify(Stream stream, string filename)
        {
            using (var br = new BinaryReaderX(stream, true))
            {
                if (br.ReadString(4) == "CLYT") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _bclyt = BCLYT.Load(FileInfo.OpenRead(), filename);
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            try
            {
                //_bclyt.Save(FileInfo.FullName);
            }
            catch (Exception) { }
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => new List<BitmapInfo> { new BitmapInfo { Bitmap = _bclyt } };

        public bool ShowProperties(Icon icon) => false;
    }
}
