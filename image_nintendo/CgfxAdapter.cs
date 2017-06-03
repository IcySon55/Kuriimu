using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kuriimu.Contract;
using System.IO;
using Kuriimu.IO;
using System.Drawing;

namespace image_nintendo.CGFX
{
    public class BcfnxAdapter : IImageAdapter
    {
        private CGFX _cgfx = null;

        #region Properties

        // Information
        public string Name => "CGFX";
        public string Description => "CTR GFX";
        public string Extension => "*.bcres";
        public string About => "This is the CGFX image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            try
            {
                using (var br = new BinaryReaderX(File.OpenRead(filename)))
                {
                    if (br.ReadString(4) == "CGFX") return false;
                    br.BaseStream.Position = 0x24;
                    return br.ReadUInt32() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
                _cgfx = new CGFX(FileInfo.OpenRead());
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            try
            {
                //_cgfx.Save(FileInfo.Create());
            }
            catch (Exception) { }
        }

        // Bitmaps
        public Bitmap Bitmap
        {
            get => _cgfx.bmps[0];
            set => _cgfx.bmps[0] = value;
        }

        public bool ShowProperties(Icon icon) => false;
    }
}
