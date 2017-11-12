using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Cetera.Image;
using Kukkii.Properties;
using Kontract.IO;
using Kontract;

namespace Kukkii
{
    public partial class OpenRaw : Form
    {
        private string _filename = string.Empty;

        public OpenRaw(string filename)
        {
            InitializeComponent();

            _filename = filename;
        }

        private void OpenRaw_Load(object sender, EventArgs e)
        {
            Icon = Resources.kukkii;

            imbPreview.GridColor = Settings.Default.GridColor1;
            imbPreview.GridColorAlternate = Settings.Default.GridColor2;
            imbPreview.ImageBorderStyle = Settings.Default.ImageBorderStyle;
            imbPreview.ImageBorderColor = Settings.Default.ImageBorderColor;

            lblFilename.Text = Path.GetFileName(_filename);

            numWidth.ValueChanged -= numWidth_ValueChanged;
            numWidth.Value = Settings.Default.OpenRawWidth;
            numWidth.ValueChanged += numWidth_ValueChanged;

            numHeight.ValueChanged -= numHeight_ValueChanged;
            numHeight.Value = Settings.Default.OpenRawHeight;
            numHeight.ValueChanged += numHeight_ValueChanged;

            cmbFormat.SelectedIndexChanged -= cmbPixelFormat_SelectedIndexChanged;
            cmbFormat.DisplayMember = "Text";
            cmbFormat.ValueMember = "Value";
            cmbFormat.DataSource = Enum.GetNames(typeof(Format)).Select(o => new ListItem(o, o)).ToList();
            cmbFormat.SelectedIndex = Enum.GetNames(typeof(Format)).ToList().IndexOf(Settings.Default.OpenRawFormat);
            cmbFormat.SelectedIndexChanged += cmbPixelFormat_SelectedIndexChanged;

            chkZOrder.Checked = Settings.Default.OpenZOrder;

            tileSize.ValueChanged -= tileSize_ValueChanged;
            tileSize.Value = Settings.Default.OpenTileSize;
            tileSize.ValueChanged += tileSize_ValueChanged;

            cmbOrientation.SelectedIndexChanged -= cmbOrientation_SelectedIndexChanged;
            cmbOrientation.DisplayMember = "Text";
            cmbOrientation.ValueMember = "Value";
            cmbOrientation.DataSource = Enum.GetNames(typeof(Cetera.Image.Orientation)).Select(o => new ListItem(o, o)).ToList();
            cmbOrientation.SelectedIndex = Enum.GetNames(typeof(Cetera.Image.Orientation)).ToList().IndexOf(Settings.Default.OpenRawOrientation);
            cmbOrientation.SelectedIndexChanged += cmbOrientation_SelectedIndexChanged;

            UpdatePreview();
        }

        private void UpdatePreview()
        {
            try
            {
                var settings = new ImageSettings();
                settings.Width = (int)numWidth.Value;
                settings.Height = (int)numHeight.Value;
                settings.Format = (Format)Enum.Parse(typeof(Format), cmbFormat.SelectedValue.ToString());
                settings.ZOrder = chkZOrder.Checked;
                settings.PadToPowerOf2 = false;
                settings.TileSize = (int)tileSize.Value;
                settings.Orientation = (Cetera.Image.Orientation)Enum.Parse(typeof(Cetera.Image.Orientation), cmbOrientation.SelectedValue.ToString());

                imbPreview.Image = Common.Load(File.ReadAllBytes(_filename), settings);
            }
            catch (Exception ex)
            {
                imbPreview.Image = null;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void numWidth_ValueChanged(object sender, EventArgs e)
        {
            Settings.Default.OpenRawWidth = numWidth.Value;
            Settings.Default.Save();
            UpdatePreview();
        }

        private void numHeight_ValueChanged(object sender, EventArgs e)
        {
            Settings.Default.OpenRawHeight = numHeight.Value;
            Settings.Default.Save();
            UpdatePreview();
        }

        private void cmbPixelFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Default.OpenRawFormat = cmbFormat.SelectedValue.ToString();
            Settings.Default.Save();
            UpdatePreview();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                InitialDirectory = Settings.Default.LastDirectory,
                FileName = Path.Combine(Path.GetDirectoryName(_filename), Path.GetFileNameWithoutExtension(_filename) + ".png"),
                Filter = "Portable Network Graphics (*.png)|*.png"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                imbPreview.Image.Save(sfd.FileName, ImageFormat.Png);
            }
        }

        private void chkZOrder_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.OpenZOrder = chkZOrder.Checked;
            Settings.Default.Save();
            UpdatePreview();
        }

        private void tileSize_ValueChanged(object sender, EventArgs e)
        {
            Settings.Default.OpenTileSize = tileSize.Value;
            Settings.Default.Save();
            UpdatePreview();
        }

        private void btnSaveRaw_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                InitialDirectory = Settings.Default.LastDirectory,
                FileName = Path.Combine(Path.GetDirectoryName(_filename), Path.GetFileNameWithoutExtension(_filename) + ".raw"),
                Filter = "Raw Image Data (*.raw)|*.raw"
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            var settings = new ImageSettings
            {
                Width = (int) numWidth.Value,
                Height = (int) numHeight.Value,
                Format = (Format) Enum.Parse(typeof(Format), cmbFormat.SelectedValue.ToString()),
                ZOrder = chkZOrder.Checked,
                PadToPowerOf2 = false,
                TileSize = (int) tileSize.Value,
                Orientation = (Cetera.Image.Orientation) Enum.Parse(typeof(Cetera.Image.Orientation), cmbOrientation.SelectedValue.ToString())
            };

            var imgSave = Common.Save((Bitmap)imbPreview.Image, settings);
            using (var bw = new BinaryWriterX(File.Create(sfd.FileName))) bw.Write(imgSave);
        }

        private void cmbOrientation_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Default.OpenRawOrientation = cmbOrientation.SelectedValue.ToString();
            Settings.Default.Save();
            UpdatePreview();
        }
    }
}