using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace image_nintendo.SMDH
{
    public partial class SmdhProperties : Form
    {
        private SMDH _smdh = null;
        private bool _hasChanges = false;

        #region Properties

        public SMDH Smdh
        {
            get { return _smdh; }
            set { _smdh = value; }
        }

        public bool HasChanges
        {
            get { return _hasChanges; }
            private set { _hasChanges = value; }
        }

        #endregion

        public SmdhProperties(SMDH smdh, Icon icon)
        {
            InitializeComponent();

            Icon = icon;
            _smdh = smdh;
        }

        private void FileProperties_Load(object sender, EventArgs e)
        {
            Text = "SMDH Extended Properties";

            // Populate Form
            List<ApplicationTitle> applicationTitles = new List<ApplicationTitle> { };
            for (int i = 0; i < 0x10; i++)
                applicationTitles.Add(new ApplicationTitle(ApplicationTitle.Languages[i]) { ShortDescription = _smdh.shortDesc[i], LongDescription = _smdh.longDesc[i], Publisher = _smdh.publisher[i] });

            bool regionFree = _smdh.appSettings.regionLockout == 0x7FFFFFFF;

            var smdhInfo = new SmdhInfo
            {
                ApplicationTitles = applicationTitles,
                MatchMakerID = _smdh.appSettings.makerID,
                MatchMakerBitID = _smdh.appSettings.makerBITID,
                EulaMinor = _smdh.appSettings.eulaVerMinor,
                EulaMajor = _smdh.appSettings.eulaVerMajor,
                OptimalAnimationDefaultFrame = _smdh.appSettings.animDefaultFrame,
                StreetPassID = _smdh.appSettings.streetPassID,

                CERO = (CERO)_smdh.appSettings.gameRating[0],
                ESRB = (ESRB)_smdh.appSettings.gameRating[1],
                // Reserved
                USK = (USK)_smdh.appSettings.gameRating[3],
                PEGI = (PEGI)_smdh.appSettings.gameRating[4],
                // Reserved
                PEGI_Portugal = (PEGI_PRT)_smdh.appSettings.gameRating[6],
                PEGI_BBFC = (PEGI_BBFC)_smdh.appSettings.gameRating[7],
                COB = (COB)_smdh.appSettings.gameRating[8],
                GRB = (GRB)_smdh.appSettings.gameRating[9],
                CGSRR = (CGSRR)_smdh.appSettings.gameRating[10],

                RegionFree = regionFree,
                Japan = ((((Regions)_smdh.appSettings.regionLockout & Regions.Japan) == Regions.Japan) || regionFree),
                NorthAmerica = ((((Regions)_smdh.appSettings.regionLockout & Regions.NorthAmerica) == Regions.NorthAmerica) || regionFree),
                Europe = ((((Regions)_smdh.appSettings.regionLockout & Regions.Europe) == Regions.Europe) || regionFree),
                Australia = ((((Regions)_smdh.appSettings.regionLockout & Regions.Australia) == Regions.Australia) || regionFree),
                China = ((((Regions)_smdh.appSettings.regionLockout & Regions.China) == Regions.China) || regionFree),
                SouthKorea = ((((Regions)_smdh.appSettings.regionLockout & Regions.SouthKorea) == Regions.SouthKorea) || regionFree),
                Taiwan = ((((Regions)_smdh.appSettings.regionLockout & Regions.Taiwan) == Regions.Taiwan) || regionFree),

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
            };

            pptExtendedProperties.SelectedObject = smdhInfo;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var smdhInfo = (SmdhInfo)pptExtendedProperties.SelectedObject;

            // Check for changes
            //if (_smdh.Property != smdhInfo.Property)
            //    _hasChanges = true;

            // Set values (incomplete)
            for (int i = 0; i < 0x10; i++)
            {
                _smdh.shortDesc[i] = smdhInfo.ApplicationTitles[i].ShortDescription;
                _smdh.longDesc[i] = smdhInfo.ApplicationTitles[i].LongDescription;
                _smdh.publisher[i] = smdhInfo.ApplicationTitles[i].Publisher;
            }

            // Temporary until change detection is added above
            _hasChanges = true;

            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}