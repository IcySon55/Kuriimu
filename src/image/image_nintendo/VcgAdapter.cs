using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.IO;
using Kuriimu.Kontract;

namespace image_nintendo.VCG
{
    public sealed class BnrAdapter : IImageAdapter
    {
        private VCG _vcg;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "VCG_VCL_VCE";
        public string Description => "Nintendo Visual Common Graphics";
        public string Extension => "*.vcg";
        public string About => "This is the VCG/VCL/VCE image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => false;
        public bool CanSave => false;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            var vcgFilename = filename;
            var vclFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".vcl");
            var vceFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".vce");

            if (!File.Exists(vcgFilename) || !File.Exists(vclFilename)) return false;

            using (var br = new BinaryReaderX(File.OpenRead(vcgFilename)))
            {
                return br.BaseStream.Length >= 4 && br.ReadString(4) == " GCV";
            }
        }

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            var vcgFilename = filename;
            var vclFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".vcl");
            var vceFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".vce");

            if (FileInfo.Exists)
            {
                _vcg = new VCG(File.OpenRead(vcgFilename), File.OpenRead(vclFilename), File.OpenRead(vceFilename));

                //if (File.Exists(vceFilename))
                //else if (File.Exists(ncerFilename))
                //    _vcg = new VCG(File.OpenRead(vcgFilename), File.OpenRead(vclFilename), File.OpenRead(ncerFilename));
                //else
                //    _vcg = new VCG(File.OpenRead(vcgFilename), File.OpenRead(vclFilename));

                _bitmaps = _vcg.bmps.Select(o => new BitmapInfo { Bitmap = o }).ToList();
            }
        }

        public void Save(string filename = "")
        {
            if (filename.Trim() != string.Empty)
                FileInfo = new FileInfo(filename);

            _vcg.bmps = _bitmaps.Select(o => o.Bitmap).ToList();
            _vcg.Save(FileInfo.FullName);
        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
