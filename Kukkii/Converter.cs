using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Cyotek.Windows.Forms;
using Kukkii.Properties;
using Kuriimu.Contract;
using Kuriimu.UI;

namespace Kukkii
{
    public partial class Converter : Form
    {
        private IImageAdapter _imageAdapter;
        private bool _fileOpen;
        private bool _hasChanges;

        private List<IImageAdapter> _imageAdapters;
        private int _selectedImageIndex = 0;

        public Converter(string[] args)
        {
            InitializeComponent();

            // Load Plugins
            _imageAdapters = PluginLoader<IImageAdapter>.LoadPlugins(Settings.Default.PluginDirectory, "image*.dll").ToList();

            // Load passed in file
            if (args.Length > 0 && File.Exists(args[0]))
                OpenFile(args[0]);
        }

        private void frmConverter_Load(object sender, EventArgs e)
        {
            Icon = Resources.kukkii;

            // Tools
            CompressionTools.LoadCompressionTools(compressionToolStripMenuItem);
            EncryptionTools.LoadEncryptionTools(encryptionToolStripMenuItem);

            UpdateForm();
            UpdatePreview();
        }

        // Menu/Toolbar
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfirmOpenFile();
        }
        private void tsbOpen_Click(object sender, EventArgs e)
        {
            ConfirmOpenFile();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile();
        }
        private void tsbSave_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile(true);
        }
        private void tsbSaveAs_Click(object sender, EventArgs e)
        {
            SaveFile(true);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void exportPNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportPNG();
        }
        private void tsbExportPNG_Click(object sender, EventArgs e)
        {
            ExportPNG();
        }

        private void importPNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportPNG();
        }
        private void tsbImportPNG_Click(object sender, EventArgs e)
        {
            ImportPNG();
        }

        // File Handling
        private void frmConverter_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void frmConverter_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (files.Length > 0 && File.Exists(files[0]))
                if (!_fileOpen)
                    ConfirmOpenFile(files[0]);
                else
                {
                    if (new FileInfo(files[0]).Extension == ".png")
                        Import(files[0]);
                    else
                        ConfirmOpenFile(files[0]);
                }
        }

        private void ConfirmOpenFile(string filename = "")
        {
            DialogResult dr = DialogResult.No;

            if (_fileOpen && _hasChanges)
                dr = MessageBox.Show($"You have unsaved changes in {FileName()}. Save changes before opening another file?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

            switch (dr)
            {
                case DialogResult.Yes:
                    dr = SaveFile();
                    if (dr == DialogResult.OK) OpenFile(filename);
                    break;
                case DialogResult.No:
                    OpenFile(filename);
                    break;
            }
        }

        private void OpenFile(string filename = "")
        {
            var ofd = new OpenFileDialog
            {
                InitialDirectory = Settings.Default.LastDirectory,
                Filter = Tools.LoadFilters(_imageAdapters)
            };

            var dr = DialogResult.OK;

            if (filename == string.Empty)
                dr = ofd.ShowDialog();

            if (dr != DialogResult.OK) return;

            if (filename == string.Empty)
                filename = ofd.FileName;

            var tempAdapter = SelectImageAdapter(filename);

            try
            {
                if (tempAdapter != null)
                {
                    tempAdapter.Load(filename);

                    _imageAdapter = tempAdapter;
                    _fileOpen = true;
                    _hasChanges = false;
                    imbPreview.Zoom = 100;

                    UpdatePreview();
                    if (treBitmaps.Nodes.Count == 0)
                    {
                        MessageBox.Show(this, $"{FileName()} was loaded by the \"{tempAdapter.Description}\" adapter but it provided no images.", "Supported Format Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _imageAdapter = null;
                        _fileOpen = false;
                    }
                    UpdateForm();
                }

                Settings.Default.LastDirectory = new FileInfo(filename).DirectoryName;
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), tempAdapter != null ? $"{tempAdapter.Name} - {tempAdapter.Description} Adapter" : "Supported Format Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DialogResult SaveFile(bool saveAs = false)
        {
            var sfd = new SaveFileDialog();
            var dr = DialogResult.OK;

            sfd.Title = $"Save as {_imageAdapter.Description}";
            sfd.FileName = _imageAdapter.FileInfo.Name;
            sfd.Filter = _imageAdapter.Description + " (" + _imageAdapter.Extension + ")|" + _imageAdapter.Extension;

            if (_imageAdapter.FileInfo == null || saveAs)
            {
                sfd.InitialDirectory = Settings.Default.LastDirectory;
                dr = sfd.ShowDialog();
            }

            if ((_imageAdapter.FileInfo == null || saveAs) && dr == DialogResult.OK)
            {
                _imageAdapter.FileInfo = new FileInfo(sfd.FileName);
                Settings.Default.LastDirectory = new FileInfo(sfd.FileName).DirectoryName;
                Settings.Default.Save();
            }

            if (dr != DialogResult.OK) return dr;

            try
            {
                _imageAdapter.Save(saveAs ? _imageAdapter.FileInfo?.FullName : string.Empty);
                _hasChanges = false;
                UpdatePreview();
                UpdateForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), _imageAdapter != null ? $"{_imageAdapter.Name} - {_imageAdapter.Description} Adapter" : "Supported Format Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dr;
        }

        private IImageAdapter SelectImageAdapter(string filename, bool batchMode = false)
        {
            IImageAdapter result = null;

            // first look for adapters whose extension matches that of our filename
            List<IImageAdapter> matchingAdapters = _imageAdapters.Where(adapter => adapter.Extension.Split(';').Any(s => filename.ToLower().EndsWith(s))).ToList();

            result = matchingAdapters.FirstOrDefault(adapter => adapter.Identify(filename));

            // if none of them match, then try all other adapters
            if (result == null)
                result = _imageAdapters.Except(matchingAdapters).FirstOrDefault(adapter => adapter.Identify(filename));

            if (result == null && !batchMode)
                MessageBox.Show("None of the installed plugins are able to open the file.", "Unsupported Format", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return result == null ? null : (IImageAdapter)Activator.CreateInstance(result.GetType());
        }

        private void ExportPNG()
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Title = "Export PNG...",
                InitialDirectory = Settings.Default.LastDirectory,
                FileName = _imageAdapter.FileInfo.Name + ".png",
                Filter = "Portable Network Graphics (*.png)|*.png",
                AddExtension = true
            };

            if (sfd.ShowDialog() == DialogResult.OK)
                _imageAdapter.Bitmaps[_selectedImageIndex].Bitmap.Save(sfd.FileName, ImageFormat.Png);
        }

        private void ImportPNG()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Import PNG...",
                InitialDirectory = Settings.Default.LastDirectory,
                Filter = "Portable Network Graphics (*.png)|*.png"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
                Import(ofd.FileName);
        }

        private void Import(string filename)
        {
            try
            {
                var bmp = (Bitmap)Image.FromFile(filename);
                _imageAdapter.Bitmaps[_selectedImageIndex].Bitmap = bmp;
                UpdatePreview();
                MessageBox.Show(filename + " imported successfully.", "Import Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdatePreview()
        {
            imbPreview.Image = _imageAdapter?.Bitmaps[_selectedImageIndex].Bitmap;

            imbPreview.GridColor = Settings.Default.GridColor;
            var gcBitmap = new Bitmap(16, 16, PixelFormat.Format24bppRgb);
            var gfx = Graphics.FromImage(gcBitmap);
            gfx.FillRectangle(new SolidBrush(Settings.Default.GridColor), 0, 0, 16, 16);
            tsbGridColor.Image = gcBitmap;

            imbPreview.GridColorAlternate = Settings.Default.GridColorAlternate;
            var gcaBitmap = new Bitmap(16, 16, PixelFormat.Format24bppRgb);
            gfx = Graphics.FromImage(gcaBitmap);
            gfx.FillRectangle(new SolidBrush(Settings.Default.GridColorAlternate), 0, 0, 16, 16);
            tsbGridColorAlternate.Image = gcaBitmap;

            UpdateImageList();
        }

        private void UpdateImageList()
        {
            if (_imageAdapter.Bitmaps?.Count <= 0) return;

            treBitmaps.BeginUpdate();
            treBitmaps.Nodes.Clear();
            imlBitmaps.Images.Clear();
            imlBitmaps.TransparentColor = Color.Transparent;
            imlBitmaps.ImageSize = new Size(Settings.Default.ThumbnailWidth, Settings.Default.ThumbnailHeight);
            treBitmaps.ItemHeight = Settings.Default.ThumbnailHeight + 6;

            for (var i = 0; i < _imageAdapter.Bitmaps.Count; i++)
            {
                var bitmapInfo = _imageAdapter.Bitmaps[i];
                if (bitmapInfo.Bitmap == null) continue;
                pptImageProperties.SelectedObject = bitmapInfo;
                imlBitmaps.Images.Add(i.ToString(), GenerateThumbnail(bitmapInfo.Bitmap));
                treBitmaps.Nodes.Add(new TreeNode
                {
                    Text = (i + 1).ToString("00"),
                    Tag = i,
                    ImageIndex = i
                });
            }

            if (treBitmaps.Nodes.Count > 0)
                treBitmaps.SelectedNode = treBitmaps.Nodes[_selectedImageIndex];

            treBitmaps.EndUpdate();
        }

        private Bitmap GenerateThumbnail(Bitmap input)
        {
            var thumbWidth = Settings.Default.ThumbnailWidth;
            var thumbHeight = Settings.Default.ThumbnailHeight;
            var thumb = new Bitmap(thumbWidth, thumbHeight, PixelFormat.Format32bppArgb);
            var gfx = Graphics.FromImage(thumb);

            gfx.CompositingQuality = CompositingQuality.HighQuality;
            gfx.PixelOffsetMode = PixelOffsetMode.HighQuality;
            gfx.SmoothingMode = SmoothingMode.HighQuality;
            gfx.InterpolationMode = InterpolationMode.Default;

            var wRatio = (float)input.Width / thumbWidth;
            var hRatio = (float)input.Height / thumbHeight;
            var ratio = wRatio >= hRatio ? wRatio : hRatio;

            if (input.Width <= thumbWidth && input.Height <= thumbHeight)
                ratio = 1.0f;

            var size = new SizeF(Math.Min(input.Width / ratio, thumbWidth), Math.Min(input.Height / ratio, thumbHeight));
            var pos = new PointF((float)thumbWidth / 2 - size.Width / 2, (float)thumbHeight / 2 - size.Height / 2);

            // Grid
            var xCount = Settings.Default.ThumbnailWidth / 16 + 1;
            var yCount = Settings.Default.ThumbnailHeight / 16 + 1;

            for (var i = 0; i < xCount; i++)
                for (var j = 0; j < yCount; j++)
                    gfx.FillRectangle(new SolidBrush((i + j) % 2 == 1 ? Settings.Default.GridColor : Settings.Default.GridColorAlternate), i * 16, j * 16, 16, 16);

            gfx.DrawImage(input, pos.X, pos.Y, size.Width, size.Height);

            return thumb;
        }

        private void UpdateForm()
        {
            Text = $"{Settings.Default.ApplicationName} v{FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion}" + (FileName() != string.Empty ? " - " + FileName() : string.Empty) + (_hasChanges ? "*" : string.Empty) + (_imageAdapter != null ? " - " + _imageAdapter.Description + " Adapter (" + _imageAdapter.Name + ")" : string.Empty);

            if (_imageAdapter != null)
            {
                // Menu
                saveToolStripMenuItem.Enabled = _fileOpen && _imageAdapter.CanSave;
                tsbSave.Enabled = _fileOpen && _imageAdapter.CanSave;
                saveAsToolStripMenuItem.Enabled = _fileOpen && _imageAdapter.CanSave;
                tsbSaveAs.Enabled = _fileOpen && _imageAdapter.CanSave;

                // Toolbar
                exportPNGToolStripMenuItem.Enabled = _fileOpen;
                tsbExportPNG.Enabled = _fileOpen;
                importPNGToolStripMenuItem.Enabled = _fileOpen;
                tsbImportPNG.Enabled = _fileOpen;
            }

            // Batch Import/Export
            batchScanSubdirectoriesToolStripMenuItem.Text = Settings.Default.BatchScanSubdirectories ? "Scan Sub-directories" : "Don't Scan Sub-directories";
            batchScanSubdirectoriesToolStripMenuItem.Image = Settings.Default.BatchScanSubdirectories ? Resources.menu_scan_subdirectories_on : Resources.menu_scan_subdirectories_off;
            tsbBatchScanSubdirectories.Text = batchScanSubdirectoriesToolStripMenuItem.Text;
            tsbBatchScanSubdirectories.Image = batchScanSubdirectoriesToolStripMenuItem.Image;
        }

        private string FileName()
        {
            return _imageAdapter == null || _imageAdapter.FileInfo == null ? string.Empty : _imageAdapter.FileInfo.Name;
        }

        // Toolbar Features
        private void batchExportPNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tsbBatchExport_Click(sender, e);
        }

        private void tsbBatchExport_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select the source directory containing image files..." + (Settings.Default.BatchScanSubdirectories ? "\r\nSub-directories will be scanned." : string.Empty);
            fbd.SelectedPath = Settings.Default.LastBatchDirectory == string.Empty ? Settings.Default.LastDirectory : Settings.Default.LastBatchDirectory;
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string path = fbd.SelectedPath;
                int count = 0;

                if (Directory.Exists(path))
                {
                    string[] types = _imageAdapters.Select(x => x.Extension.ToLower()).ToArray();

                    List<string> files = new List<string>();
                    foreach (string type in types)
                    {
                        string[] subTypes = type.Split(';');
                        foreach (string subType in subTypes)
                            files.AddRange(Directory.GetFiles(path, subType, (Settings.Default.BatchScanSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)));
                    }

                    foreach (string file in files)
                    {
                        if (File.Exists(file))
                        {
                            FileInfo fi = new FileInfo(file);
                            IImageAdapter currentAdapter = SelectImageAdapter(file, true);

                            if (currentAdapter != null)
                            {
                                currentAdapter.Load(file);
                                currentAdapter.Bitmaps[_selectedImageIndex].Bitmap.Save(fi.FullName + ".png"); // TODO: Multi-Image support
                                count++;
                            }
                        }
                    }

                    MessageBox.Show("Batch export completed successfully. " + count + " image(s) succesfully exported.", "Batch Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            Settings.Default.LastBatchDirectory = fbd.SelectedPath;
            Settings.Default.Save();
        }

        private void batchImportPNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tsbBatchImport_Click(sender, e);
        }

        private void tsbBatchImport_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select the source directory containing image and png file pairs..." + (Settings.Default.BatchScanSubdirectories ? "\r\nSub-directories will be scanned." : string.Empty);
            fbd.SelectedPath = Settings.Default.LastBatchDirectory == string.Empty ? Settings.Default.LastDirectory : Settings.Default.LastBatchDirectory;
            fbd.ShowNewFolderButton = false;

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string path = fbd.SelectedPath;
                int fileCount = 0;
                int importCount = 0;

                if (Directory.Exists(path))
                {
                    string[] types = _imageAdapters.Select(x => x.Extension.ToLower()).ToArray();

                    List<string> files = new List<string>();
                    foreach (string type in types)
                    {
                        string[] subTypes = type.Split(';');
                        foreach (string subType in subTypes)
                            files.AddRange(Directory.GetFiles(path, subType, (Settings.Default.BatchScanSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)));
                    }

                    foreach (string file in files)
                    {
                        if (File.Exists(file))
                        {
                            FileInfo fi = new FileInfo(file);
                            IImageAdapter currentAdapter = SelectImageAdapter(file, true);

                            if (currentAdapter != null && currentAdapter.CanSave && File.Exists(fi.FullName + ".png"))
                                try
                                {
                                    var bmp = (Bitmap)Image.FromFile(fi.FullName + ".png");
                                    currentAdapter.Load(file);
                                    currentAdapter.Bitmaps[_selectedImageIndex].Bitmap = bmp; // TODO: Multi-Image support
                                    currentAdapter.Save();
                                    importCount++;
                                }
                                catch (Exception) { }

                            fileCount++;
                        }
                    }

                    MessageBox.Show("Batch import completed successfully. " + importCount + " of " + fileCount + " files succesfully imported.", "Batch Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            Settings.Default.LastBatchDirectory = fbd.SelectedPath;
            Settings.Default.Save();
        }

        private void batchScanSubdirectoriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tsbBatchScanSubdirectories_Click(sender, e);
        }

        private void tsbBatchScanSubdirectories_Click(object sender, EventArgs e)
        {
            Settings.Default.BatchScanSubdirectories = !Settings.Default.BatchScanSubdirectories;
            Settings.Default.Save();
            UpdateForm();
        }

        // Grid Colors
        private void tsbGridColor_Click(object sender, EventArgs e)
        {
            clrDialog.Color = imbPreview.GridColor;
            if (clrDialog.ShowDialog() != DialogResult.OK) return;

            imbPreview.GridColor = clrDialog.Color;
            Settings.Default.GridColor = clrDialog.Color;
            Settings.Default.Save();
            UpdatePreview();
        }

        private void tsbGridColorAlternate_Click(object sender, EventArgs e)
        {
            clrDialog.Color = imbPreview.GridColorAlternate;
            if (clrDialog.ShowDialog() != DialogResult.OK) return;

            imbPreview.GridColorAlternate = clrDialog.Color;
            Settings.Default.GridColorAlternate = clrDialog.Color;
            Settings.Default.Save();
            UpdatePreview();
        }

        // Apps
        private void tsbKuriimu_Click(object sender, EventArgs e)
        {
            ProcessStartInfo start = new ProcessStartInfo(Path.Combine(Application.StartupPath, "kuriimu.exe"));
            start.WorkingDirectory = Application.StartupPath;

            Process p = new Process();
            p.StartInfo = start;
            p.Start();
        }

        private void tsbKarameru_Click(object sender, EventArgs e)
        {
            ProcessStartInfo start = new ProcessStartInfo(Path.Combine(Application.StartupPath, "karameru.exe"));
            start.WorkingDirectory = Application.StartupPath;

            Process p = new Process();
            p.StartInfo = start;
            p.Start();
        }

        // Image Box
        private void imbPreview_Zoomed(object sender, ImageBoxZoomEventArgs e)
        {
            tslZoom.Text = "Zoom: " + imbPreview.Zoom + "%";
        }

        private void imbPreview_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                imbPreview.SelectionMode = ImageBoxSelectionMode.None;
                imbPreview.Cursor = Cursors.SizeAll;
                tslTool.Text = "Tool: Pan";
            }
        }

        private void imbPreview_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                imbPreview.SelectionMode = ImageBoxSelectionMode.Zoom;
                imbPreview.Cursor = Cursors.Default;
                tslTool.Text = "Tool: Zoom";
            }
        }

        // Info Controls
        private void treBitmaps_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _selectedImageIndex = treBitmaps.Nodes.IndexOf(treBitmaps.SelectedNode);
            UpdatePreview();
        }
    }
}
