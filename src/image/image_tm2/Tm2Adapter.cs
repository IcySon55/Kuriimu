using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using Kontract.Interface;
using Komponent.IO;

namespace image_tm2
{
    [FilePluginMetadata(Name = "TM2", Description = "Texture Matrix 2", Extension = "*.bip",
        Author = "onepiecefreak", About = "This is the TM2 image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class Tm2Adapter : IImageAdapter
    {
        private TM2 _tm2 = null;
        private List<BitmapInfo> _bitmaps;

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
                try
                {
                    if (br.BaseStream.Length < 4) return Identification.False;
                    int count = br.ReadInt32();
                    if (br.BaseStream.Length < count * 0x4) return Identification.False;
                    br.BaseStream.Position = (count - 1) * 0x4;
                    int off = br.ReadInt32();
                    if (br.BaseStream.Length < off + 8) return Identification.False;
                    br.BaseStream.Position = off;
                    if (br.ReadString(8) == "EMUARC__") return Identification.True; ;
                }
                catch (Exception)
                {
                    return Identification.False;
                }

                return Identification.False;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _tm2 = new TM2(FileInfo.OpenRead());

                _bitmaps = new List<BitmapInfo> { new BitmapInfo { Bitmap = _tm2.bmp } };
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _tm2.bmp = _bitmaps[0].Bitmap;
            //_tm2.Save(FileInfo.FullName);
        }

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
