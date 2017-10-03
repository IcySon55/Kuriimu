using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.ComponentModel;

namespace Kuriimu.Kontract.Interface
{
    public interface ISoundAdapter : IPlugin
    {
        // Feature Support
        bool FileHasExtendedProperties { get; } // Format provides an extended properties dialog?
        bool CanSave { get; } // Is saving supported?

        // I/O
        FileInfo FileInfo { get; set; }
        bool Identify(string filename); // Determines if the given file is opened by the plugin.
        void Load(string filename);
        void Save(string filename = ""); // A non-blank filename is provided when using Save As...

        // Sound
        AudioInfo Audio { get; }

        // Features
        bool ShowProperties(Icon icon);
    }

    public class AudioInfo
    {
        [Browsable(false)]
        public byte[] AudioData { get; set; }

        [Category("Properties")]
        [ReadOnly(true)]
        public string Name { get; set; }

        [Category("Properties")]
        [Description("Samplerate of the audio.")]
        public int Samplerate { get; set; }

        [Category("Properties")]
        [Description("Number of channels.")]
        public int Channels { get; set; }
    }
}
