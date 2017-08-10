using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kuriimu.Kontract;
using Kuriimu.IO;

namespace image_nintendo.SMDH
{
    public sealed class IcnAdapter : IImageAdapter
    {
        private SMDH _smdh;
        private List<BitmapInfo> _bitmaps;

        #region Properties

        public string Name => "SMDH";
        public string Description => "SMDH Icon";
        public string Extension => "*.icn;*.bin";
        public string About => "This is the SMDH image adapter for Kukkii.";

        // Feature Support
        public bool FileHasExtendedProperties => true;
        public bool CanSave => true;

        public FileInfo FileInfo { get; set; }

        #endregion

        public bool Identify(string filename)
        {
            using (var br = new BinaryReaderX(File.OpenRead(filename)))
            {
                if (br.BaseStream.Length < 4) return false;
                return br.ReadString(4) == "SMDH";
            }
        }

        public List<string> Languages = new List<string> { "Japanese", "English", "French", "German", "Italian", "Spanish", "SimplifiedChinese", "Korean", "Dutch", "Portuguese", "Russian", "TraditionalChinese", "UnknownLanguage1", "UnknownLanguage2", "UnknownLanguage3", "UnknownLanguage4" };

        public void Load(string filename)
        {
            FileInfo = new FileInfo(filename);

            if (FileInfo.Exists)
            {
                _smdh = new SMDH(FileInfo.OpenRead());

                List<ApplicationTitle> _ApplicationTitles = new List<ApplicationTitle> { };
                for (int i = 0; i < 0x10; i++) {
                    _ApplicationTitles.Add(new ApplicationTitle(Languages[i]) { ShortDescription = _smdh.shortDesc[i], LongDescription = _smdh.longDesc[i], Publisher = _smdh.publisher[i] });
                }

                bool _RegionFree = (_smdh.appSettings.regionLockout == 0x7FFFFFFF);
                _bitmaps = _smdh.bmps.Select(o => new SMDHBitmapInfo {
                    Bitmap = o,
                    ApplicationTitles = _ApplicationTitles,
                    MatchMakerID = _smdh.appSettings.makerID,
                    MatchMakerBitID = _smdh.appSettings.makerBITID,
                    EulaMinor = _smdh.appSettings.eulaVerMinor,
                    EulaMajor = _smdh.appSettings.eulaVerMajor,
                    OptimalAnimationDefaultFrame = _smdh.appSettings.animDefaultFrame,
                    StreetPassID = _smdh.appSettings.streetPassID,

                    CERO = (Ratings.CERO)_smdh.appSettings.gameRating[0],
                    ESRB = (Ratings.ESRB)_smdh.appSettings.gameRating[1],
                    // Reserved
                    USK = (Ratings.USK)_smdh.appSettings.gameRating[3],
                    PEGI = (Ratings.PEGI)_smdh.appSettings.gameRating[4],
                    // Reserved
                    PEGI_Portugal = (Ratings.PEGI_PRT)_smdh.appSettings.gameRating[6],
                    PEGI_BBFC = (Ratings.PEGI_BBFC)_smdh.appSettings.gameRating[7],
                    COB = (Ratings.COB)_smdh.appSettings.gameRating[8],
                    GRB = (Ratings.GRB)_smdh.appSettings.gameRating[9],
                    CGSRR = (Ratings.CGSRR)_smdh.appSettings.gameRating[10],

                    RegionFree = _RegionFree,
                    Japan = ((((Region)_smdh.appSettings.regionLockout & Region.Japan) == Region.Japan) || _RegionFree),
                    NorthAmerica = ((((Region)_smdh.appSettings.regionLockout & Region.NorthAmerica) == Region.NorthAmerica) || _RegionFree),
                    Europe = ((((Region)_smdh.appSettings.regionLockout & Region.Europe) == Region.Europe) || _RegionFree),
                    Australia = ((((Region)_smdh.appSettings.regionLockout & Region.Australia) == Region.Australia) || _RegionFree),
                    China = ((((Region)_smdh.appSettings.regionLockout & Region.China) == Region.China) || _RegionFree),
                    SouthKorea = ((((Region)_smdh.appSettings.regionLockout & Region.SouthKorea) == Region.SouthKorea) || _RegionFree),
                    Taiwan = ((((Region)_smdh.appSettings.regionLockout & Region.Taiwan) == Region.Taiwan) || _RegionFree),

                    Visible = (((Flags)_smdh.appSettings.flags & Flags.Visible) == Flags.Visible),
                    AutoBoot = (((Flags)_smdh.appSettings.flags & Flags.AutoBoot) == Flags.AutoBoot),
                    Allow3D = (((Flags)_smdh.appSettings.flags & Flags.Allow3D) == Flags.Allow3D),
                    RequireEULA = (((Flags)_smdh.appSettings.flags & Flags.RequireEULA) == Flags.RequireEULA),
                    AutoSave = (((Flags)_smdh.appSettings.flags & Flags.AutoSave) == Flags.AutoSave),
                    UseExtendedBanner = (((Flags)_smdh.appSettings.flags & Flags.UseExtendedBanner) == Flags.UseExtendedBanner),
                    RegionRatingRequired = (((Flags)_smdh.appSettings.flags & Flags.RegionRatingRequired) == Flags.RegionRatingRequired),
                    UsesSaveData = (((Flags)_smdh.appSettings.flags & Flags.UsesSaveData) == Flags.UsesSaveData),
                    RecordApplicationUse = (((Flags)_smdh.appSettings.flags & Flags.RecordApplicationUse) == Flags.RecordApplicationUse),
                    DisableSDSaveBackup = (((Flags)_smdh.appSettings.flags & Flags.DisableSDSaveBackup) == Flags.DisableSDSaveBackup),
                    New3DSExclusive = (((Flags)_smdh.appSettings.flags & Flags.New3DSExclusive) == Flags.New3DSExclusive)
                }).ToList<BitmapInfo>();
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
