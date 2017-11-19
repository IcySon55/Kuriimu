using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using Komponent.IO;
using Kontract.Interface;

namespace image_nintendo.VCG
{
    [FilePluginMetadata(Name = "VCG_VCL_VCE", Description = "Nintendo Visual Common Graphics", Extension = "*.vcg",
        Author = "IcySon55", About = "This is the VCG_VCL_VCE image adapter for Kukkii.")]
    [Export(typeof(IImageAdapter))]
    public sealed class BnrAdapter : IImageAdapter
    {
        private VCG _vcg;
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
            var vcgFilename = filename;
            var vclFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".vcl");
            var vceFilename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".vce");

            if (!File.Exists(vcgFilename) || !File.Exists(vclFilename)) return Identification.False;

            using (var br = new BinaryReaderX(stream, true))
            {
                if (br.BaseStream.Length >= 4 && br.ReadString(4) == " GCV") return Identification.True;
            }

            return Identification.False;
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

        public void New()
        {

        }

        // Bitmaps
        public IList<BitmapInfo> Bitmaps => _bitmaps;

        public bool ShowProperties(Icon icon) => false;
    }
}
