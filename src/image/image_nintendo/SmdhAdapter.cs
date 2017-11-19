using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using Kontract.Interface;
using Komponent.Image.Format;
using Komponent.IO;

namespace image_nintendo.SMDH
{
    [FilePluginMetadata(Name = "SMDH", Description = "SMDH Icon", Extension = "*.icn;*.bin",
        Author = "onepiecefreak,uwabami", About = "This is the SMDH image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class IcnAdapter : IImageAdapter
    {
        private SMDH _smdh;
        private List<BitmapInfo> _bitmaps;

        #region Properties
        // Feature Support
        public bool FileHasExtendedProperties => true;
        public bool CanSave => true;
        public bool CanCreateNew => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public Identification Identify(Stream stream, string filename)
        {
            using (var br = new BinaryReaderX(stream, true))
            {
                if (br.BaseStream.Length < 4) return Identification.False;
                if (br.ReadString(4) == "SMDH") return Identification.True;
            }

            return Identification.False;
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _smdh = new SMDH(FileInfo.OpenRead());
                _bitmaps = _smdh.bmps.Select(o => new SMDHBitmapInfo { Bitmap = o, Format = new RGBA(5, 6, 5).FormatName }).ToList<BitmapInfo>();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            var stream = FileInfo.Create();
            try
            {
                _smdh.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
                _smdh.Save(stream);
            }
            catch (Exception)
            {
                stream.Close();
                FileInfo.Delete();
                throw;
            }
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon)
        {
            var extendedProperties = new SmdhProperties(_smdh, icon);
            extendedProperties.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            extendedProperties.ShowDialog();
            return extendedProperties.HasChanges;
        }
    }
}
