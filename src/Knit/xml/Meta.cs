using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Knit
{
    [XmlRoot("meta")]
    public sealed class Meta
    {
        #region Properties

        [XmlElement("title")]
        public string Title { get; set; } = "Knit";

        [XmlElement("author")]
        public string Author { get; set; } = string.Empty;

        [XmlElement("version")]
        public string Version { get; set; } = string.Empty;

        [XmlElement("about")]
        public string About { get; set; } = string.Empty;

        [XmlElement("website")]
        public string Website { get; set; } = string.Empty;

        [XmlElement("icon")]
        public string Icon { get; set; } = "icon.ico";

        [XmlElement("background")]
        public string Background { get; set; } = "background.png";

        [XmlElement("music")]
        public string Music { get; set; } = "theme.mp3";

        [XmlElement("layout")]
        public Layout Layout { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the Meta class that is empty.
        /// </summary>
        public Meta()
        {
            Layout = new Layout();
        }

        /// <summary>
        /// Loads a meta.xml document from disk.
        /// </summary>
        /// <param name="filename">The meta.xml to load.</param>
        /// <returns></returns>
        public static Meta Load(string filename)
        {
            try
            {
                using (var fs = File.OpenRead(filename))
                    return (Meta)new XmlSerializer(typeof(Meta)).Deserialize(XmlReader.Create(fs, new XmlReaderSettings { CheckCharacters = false }));
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Saves the meta.xml document to disk.
        /// </summary>
        /// <param name="filename">The filename to save to.</param>
        public void Save(string filename)
        {
            try
            {
                var xmlSettings = new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    Indent = true,
                    NewLineOnAttributes = false,
                    NewLineHandling = NewLineHandling.Entitize,
                    IndentChars = "	",
                    CheckCharacters = false
                };

                using (var xmlIO = new StreamWriter(filename, false, xmlSettings.Encoding))
                {
                    var serializer = new XmlSerializer(typeof(Meta));
                    var namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    serializer.Serialize(XmlWriter.Create(xmlIO, xmlSettings), this, namespaces);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    public class Layout
    {
        [XmlElement("progressBar")]
        public Appearance ProgressBar { get; set; }

        [XmlElement("patchButton")]
        public Appearance PatchButton { get; set; }

        [XmlElement("exitButton")]
        public Appearance ExitButton { get; set; }

        [XmlElement("musicButton")]
        public AppearanceMusic MusicButton { get; set; }

        [XmlElement("aboutButton")]
        public AppearanceIcon AboutButton { get; set; }

        [XmlElement("websiteButton")]
        public AppearanceIcon WebsiteButton { get; set; }

        [XmlElement("statusTextBox")]
        public AppearanceTextBox StatusTextBox { get; set; }

        public Layout()
        {
            ProgressBar = new Appearance
            {
                Width = 600,
                Height = 23
            };
            PatchButton = new Appearance("&Patch..")
            {
                Y = 23,
                Width = 75,
                Height = 23
            };
            ExitButton = new Appearance("E&xit")
            {
                X = 75,
                Y = 23,
                Width = 75,
                Height = 23
            };
            MusicButton = new AppearanceMusic
            {
                X = 75 * 2,
                Width = 23,
                Height = 23
            };
            AboutButton = new AppearanceIcon
            {
                X = 75 * 2,
                Width = 23,
                Height = 23
            };
            WebsiteButton = new AppearanceIcon
            {
                X = 75 * 2,
                Width = 23,
                Height = 23
            };
            StatusTextBox = new AppearanceTextBox
            {
                Y = 23 * 2,
                Width = 400,
                Height = 46,
                Border = BorderStyle.Fixed3D
            };
        }
    }

    public class Appearance
    {
        [XmlIgnore]
        public Color Color { get; set; } = Color.Black;

        [XmlAttribute("color")]
        public string ColorXml
        {
            get => ColorTranslator.ToHtml(Color);
            set => Color = ColorTranslator.FromHtml(value);
        }

        [XmlAttribute("text")]
        public string Text { get; set; } = string.Empty;

        [XmlAttribute("x")]
        public int X { get; set; }

        [XmlAttribute("y")]
        public int Y { get; set; }

        [XmlAttribute("width")]
        public int Width { get; set; }

        [XmlAttribute("height")]
        public int Height { get; set; }

        [XmlAttribute("anchor")]
        public AnchorStyles Anchor { get; set; }

        public Appearance() { }

        public Appearance(string text)
        {
            Text = text;
        }

        public Point Location => new Point(X, Y);

        public Size Size => new Size(Width, Height);
    }

    public class AppearanceMusic : Appearance
    {
        [XmlElement("on")]
        public string On { get; set; }

        [XmlElement("off")]
        public string Off { get; set; }
    }

    public class AppearanceIcon : Appearance
    {
        [XmlElement("icon")]
        public string Icon { get; set; }
    }

    public class AppearanceTextBox : Appearance
    {
        [XmlAttribute("border")]
        public BorderStyle Border { get; set; }

        [XmlIgnore]
        public Color BackColor { get; set; } = Color.White;

        [XmlAttribute("backColor")]
        public string BackColorXml
        {
            get => ColorTranslator.ToHtml(BackColor);
            set => BackColor = ColorTranslator.FromHtml(value);
        }
    }
}