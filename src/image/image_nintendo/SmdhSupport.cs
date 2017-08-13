using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using Cetera.Image;
using Kuriimu.Kontract;

namespace image_nintendo.SMDH
{
    public sealed class SMDHBitmapInfo : BitmapInfo
    {
        [Category("Properties")]
        [ReadOnly(true)]
        public Format Format { get; set; }
    }

    public enum CERO : byte // Japan
    {
        Undefined = 0x00,
        NoAgeRestriction = 0x20,
        Pending = 0x40,

        AllAges = 0x80,
        Age12 = 0x8C,
        Age15 = 0x8F,
        Age17 = 0x91,
        Age18 = 0x92
    }

    public enum ESRB : byte // USA / Canada
    {
        Undefined = 0x00,
        NoAgeRestriction = 0x20,
        Pending = 0x40,

        EarlyChildhood = 0x83, // (3+)
        Everyone = 0x86, // (6+)
        Everyone10 = 0x8A, // (10+)
        Teen = 0x8D, // (13+)
        Mature = 0x91, // (17+)
        AdultsOnly = 0x92 // (18+)
    }

    public enum USK : byte // Germany
    {
        Undefined = 0x00,
        NoAgeRestriction = 0x20,
        Pending = 0x40,

        Age0 = 0x80,
        Age6 = 0x86,
        Age12 = 0x8C,
        Age16 = 0x90,
        Age18 = 0x92
    }

    public enum PEGI : byte // Europe
    {
        Undefined = 0x00,
        NoAgeRestriction = 0x20,
        Pending = 0x40,

        Age3 = 0x83,
        Age7 = 0x87,
        Age12 = 0x8C,
        Age16 = 0x90,
        Age18 = 0x92
    }

    public enum PEGI_PRT : byte // Portugal
    {
        Undefined = 0x00,
        NoAgeRestriction = 0x20,
        Pending = 0x40,

        Age4 = 0x84,
        Age6 = 0x86,
        Age12 = 0x8C,
        Age16 = 0x90,
        Age18 = 0x92
    }

    public enum PEGI_BBFC : byte // UK
    {
        Undefined = 0x00,
        NoAgeRestriction = 0x20,
        Pending = 0x40,

        Age3 = 0x83,
        Universal = 0x84, // (4+)
        Age7 = 0x87,
        ParentalGuidance = 0x88, // (8+)
        Age12 = 0x8C,
        Age15 = 0x8F,
        Age16 = 0x90,
        Age18 = 0x92
    }

    // !!! NEEDS WORK !!!
    public enum COB : byte // Australia
    {
        Undefined = 0x00,
        NoAgeRestriction = 0x20,
        Pending = 0x40,

        General = 0x80, // (All Ages)
        ParentalGuidance = 0x8F, // (15+)
        Mature = 0x8F, // (15+)
        Mature15 = 0x8F, // (15+)
        Restricted = 0x92, // (18+)
    }

    public enum GRB : byte // South Korea
    {
        Undefined = 0x00,
        NoAgeRestriction = 0x20,
        Pending = 0x40,

        All = 0x80,
        Age12 = 0x8C,
        Age15 = 0x8F,
        Age18 = 0x92
    }

    public enum CGSRR : byte // Taiwan
    {
        Undefined = 0x00,
        NoAgeRestriction = 0x20,
        Pending = 0x40,

        GeneralPublic = 0x80,
        Age6 = 0x86,
        Age12 = 0x8C,
        Age15 = 0x8F,
        Age18 = 0x92
    }

    public enum Regions : int
    {
        Japan = 0x01,
        NorthAmerica = 0x02,
        Europe = 0x04,
        Australia = 0x08,
        China = 0x10,
        SouthKorea = 0x20,
        Taiwan = 0x40
    }

    public enum Flags : int
    {
        Visible = 0x0001,
        AutoBoot = 0x0002,
        Allow3D = 0x0004,
        RequireEULA = 0x0008,
        AutoSave = 0x0010,
        UseExtendedBanner = 0x0020,
        RegionRatingRequired = 0x0040,
        UsesSaveData = 0x0080,
        RecordApplicationUse = 0x0100,
        DisableSDSaveBackup = 0x0400,
        New3DSExclusive = 0x1000
    }

    public class ApplicationTitle
    {
        private string _title = string.Empty;
        public static List<string> Languages = new List<string> { "Japanese", "English", "French", "German", "Italian", "Spanish", "SimplifiedChinese", "Korean", "Dutch", "Portuguese", "Russian", "TraditionalChinese", "UnknownLanguage1", "UnknownLanguage2", "UnknownLanguage3", "UnknownLanguage4" };
        public override string ToString() => _title;

        [Category("Application Titles")]
        public string ShortDescription { get; set; }

        [Category("Application Titles")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string LongDescription { get; set; } // Multiline

        [Category("Application Titles")]
        public string Publisher { get; set; }

        public ApplicationTitle(string Title) { _title = Title; }
    }

    public class SmdhInfo
    {
        [Category("Misc"), DisplayName("Application Titles")] public List<ApplicationTitle> ApplicationTitles { get; set; }
        [Category("Misc"), DisplayName("Match Maker ID")] public Int32 MatchMakerID { get; set; }
        [Category("Misc"), DisplayName("Match Maker BIT ID")] public Int64 MatchMakerBitID { get; set; }
        [Category("Misc"), DisplayName("EULA Version Minor")] public byte EulaMinor { get; set; }
        [Category("Misc"), DisplayName("EULA Version Major")] public byte EulaMajor { get; set; }
        [Category("Misc"), DisplayName("Optimal Animation Default Frame")] public Int32 OptimalAnimationDefaultFrame { get; set; }
        [Category("Misc"), DisplayName("StreetPass ID")] public Int32 StreetPassID { get; set; }

        [Category("Ratings"), DisplayName("CERO (Japan)")] public CERO CERO { get; set; }
        [Category("Ratings"), DisplayName("ESRB (North America)")] public ESRB ESRB { get; set; }
        [Category("Ratings"), DisplayName("USK (Germany)")] public USK USK { get; set; }
        [Category("Ratings"), DisplayName("PEGI (Europe)")] public PEGI PEGI { get; set; }
        [Category("Ratings"), DisplayName("PEGI (Portugal)")] public PEGI_PRT PEGI_Portugal { get; set; }
        [Category("Ratings"), DisplayName("PEGI (BBFC, UK)")] public PEGI_BBFC PEGI_BBFC { get; set; }
        [Category("Ratings"), DisplayName("COB (Australia)")] public COB COB { get; set; }
        [Category("Ratings"), DisplayName("GRB (South Korea)")] public GRB GRB { get; set; }
        [Category("Ratings"), DisplayName("CGSRR (Taiwan)")] public CGSRR CGSRR { get; set; }

        [Category("Region Lock")] public bool Japan { get; set; }
        [Category("Region Lock"), DisplayName("North America")] public bool NorthAmerica { get; set; }
        [Category("Region Lock")] public bool Europe { get; set; }
        [Category("Region Lock")] public bool Australia { get; set; }
        [Category("Region Lock")] public bool China { get; set; }
        [Category("Region Lock"), DisplayName("South Korea")] public bool SouthKorea { get; set; }
        [Category("Region Lock")] public bool Taiwan { get; set; }
        [Category("Region Lock"), DisplayName("Region Free")] public bool RegionFree { get; set; }

        [Category("Flags")] public bool Visible { get; set; }
        [Category("Flags")] public bool AutoBoot { get; set; }
        [Category("Flags"), DisplayName("Allow 3D")] public bool Allow3D { get; set; }
        [Category("Flags"), DisplayName("Require EULA")] public bool RequireEULA { get; set; }
        [Category("Flags")] public bool AutoSave { get; set; }
        [Category("Flags"), DisplayName("Use Extended Banner")] public bool UseExtendedBanner { get; set; }
        [Category("Flags"), DisplayName("Region Rating Required")] public bool RegionRatingRequired { get; set; }
        [Category("Flags"), DisplayName("Uses Save Data")] public bool UsesSaveData { get; set; }
        [Category("Flags"), DisplayName("Record Application Use")] public bool RecordApplicationUse { get; set; }
        [Category("Flags"), DisplayName("Disable SD Save Backup")] public bool DisableSDSaveBackup { get; set; }
        [Category("Flags"), DisplayName("New 3DS Exclusive")] public bool New3DSExclusive { get; set; }
    }
}
