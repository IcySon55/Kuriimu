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
using Kontract.Interface;
using Kontract;
using Kontract.UI;
using System.Threading.Tasks;
using System.Threading;

namespace Kukkii
{
    public partial class Converter : Form
    {
        private IImageAdapter _imageAdapter;
        private bool _fileOpen;
        private bool _hasChanges;

        private List<IImageAdapter> _imageAdapters;
        private int _selectedImageIndex;
        private Bitmap _thumbnailBackground;

        Dictionary<string, string> _stylesText = new Dictionary<string, string>
        {
            ["None"] = "None",
            ["FixedSingle"] = "Simple",
            ["FixedSingleDropShadow"] = "Drop Shadow",
            ["FixedSingleGlowShadow"] = "Glow Shadow"
        };

        Dictionary<string, string> _stylesImages = new Dictionary<string, string>
        {
            ["None"] = "menu_border_none",
            ["FixedSingle"] = "menu_border_simple",
            ["FixedSingleDropShadow"] = "menu_border_drop_shadow",
            ["FixedSingleGlowShadow"] = "menu_border_glow_shadow"
        };

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
            HashTools.LoadHashTools(hashToolStripMenuItem);

            // Image Border Styles
            tsbImageBorderStyle.DropDownItems.AddRange(Enum.GetNames(typeof(ImageBoxBorderStyle)).Select(s => new ToolStripMenuItem { Image = (Image)Resources.ResourceManager.GetObject(_stylesImages[s]), Text = _stylesText[s], Tag = s }).ToArray());
            foreach (var tsb in tsbImageBorderStyle.DropDownItems)
                ((ToolStripMenuItem)tsb).Click += tsbImageBorderStyle_Click;

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

        private void openRawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.InitialDirectory = Settings.Default.LastDirectory;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                var openRaw = new OpenRaw(ofd.FileName);
                openRaw.ShowDialog();
            }
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
                    _selectedImageIndex = 0;

                    UpdatePreview();
                    UpdateImageList();
                    if (_imageAdapter.Bitmaps?.Count <= 0)
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
            List<IImageAdapter> matchingAdapters = _imageAdapters.Where(adapter => adapter.Extension.TrimEnd(';').Split(';').Any(s => filename.ToLower().EndsWith(s.TrimStart('*')))).ToList();

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
                FileName = _imageAdapter.FileInfo.Name + (_imageAdapter.Bitmaps.Count > 1 ? "." + _selectedImageIndex.ToString("00") : string.Empty) + ".png",
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
                _imageAdapter.Bitmaps[_selectedImageIndex].Bitmap = new Bitmap(filename);
                UpdatePreview();
                UpdateImageList();
                treBitmaps.SelectedNode = treBitmaps.Nodes[_selectedImageIndex];
                MessageBox.Show(filename + " imported successfully.", "Import Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdatePreview()
        {
            if (_imageAdapter?.Bitmaps.Count > 0)
            {
                imbPreview.Image = _imageAdapter?.Bitmaps[_selectedImageIndex].Bitmap;
                pptImageProperties.SelectedObject = _imageAdapter?.Bitmaps[_selectedImageIndex];
            }

            // Grid Color 1
            imbPreview.GridColor = Settings.Default.GridColor1;
            var gc1Bitmap = new Bitmap(16, 16, PixelFormat.Format24bppRgb);
            var gfx = Graphics.FromImage(gc1Bitmap);
            gfx.FillRectangle(new SolidBrush(Settings.Default.GridColor1), 0, 0, 16, 16);
            tsbGridColor1.Image = gc1Bitmap;

            // Grid Color 2
            imbPreview.GridColorAlternate = Settings.Default.GridColor2;
            var gc2Bitmap = new Bitmap(16, 16, PixelFormat.Format24bppRgb);
            gfx = Graphics.FromImage(gc2Bitmap);
            gfx.FillRectangle(new SolidBrush(Settings.Default.GridColor2), 0, 0, 16, 16);
            tsbGridColor2.Image = gc2Bitmap;

            // Image Border Style
            imbPreview.ImageBorderStyle = Settings.Default.ImageBorderStyle;
            tsbImageBorderStyle.Image = (Image)Resources.ResourceManager.GetObject(_stylesImages[Settings.Default.ImageBorderStyle.ToString()]);
            tsbImageBorderStyle.Text = _stylesText[Settings.Default.ImageBorderStyle.ToString()];

            // Image Border Color
            imbPreview.ImageBorderColor = Settings.Default.ImageBorderColor;
            var ibcBitmap = new Bitmap(16, 16, PixelFormat.Format24bppRgb);
            gfx = Graphics.FromImage(ibcBitmap);
            gfx.FillRectangle(new SolidBrush(Settings.Default.ImageBorderColor), 0, 0, 16, 16);
            tsbImageBorderColor.Image = ibcBitmap;
        }

        private void UpdateImageList()
        {
            if (_imageAdapter == null || _imageAdapter.Bitmaps?.Count <= 0) return;

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
                imlBitmaps.Images.Add(i.ToString(), GenerateThumbnail(bitmapInfo.Bitmap));
                treBitmaps.Nodes.Add(new TreeNode
                {
                    Text = !string.IsNullOrEmpty(bitmapInfo.Name) ? bitmapInfo.Name : i.ToString("00"),
                    Tag = i,
                    ImageKey = i.ToString(),
                    SelectedImageKey = i.ToString()
                });
            }

            treBitmaps.EndUpdate();
        }

        private Bitmap GenerateThumbnail(Bitmap input)
        {
            var thumbWidth = Settings.Default.ThumbnailWidth;
            var thumbHeight = Settings.Default.ThumbnailHeight;
            var thumb = new Bitmap(thumbWidth, thumbHeight, PixelFormat.Format24bppRgb);
            var gfx = Graphics.FromImage(thumb);

            gfx.CompositingQuality = CompositingQuality.HighSpeed;
            gfx.PixelOffsetMode = PixelOffsetMode.Default;
            gfx.SmoothingMode = SmoothingMode.HighSpeed;
            gfx.InterpolationMode = InterpolationMode.Default;

            var wRatio = (float)input.Width / thumbWidth;
            var hRatio = (float)input.Height / thumbHeight;
            var ratio = wRatio >= hRatio ? wRatio : hRatio;

            if (input.Width <= thumbWidth && input.Height <= thumbHeight)
                ratio = 1.0f;

            var size = new Size((int)Math.Min(input.Width / ratio, thumbWidth), (int)Math.Min(input.Height / ratio, thumbHeight));
            var pos = new Point(thumbWidth / 2 - size.Width / 2, thumbHeight / 2 - size.Height / 2);

            // Grid
            if (_thumbnailBackground == null)
                GenerateThumbnailBackground();

            gfx.DrawImageUnscaled(_thumbnailBackground, 0, 0, _thumbnailBackground.Width, _thumbnailBackground.Height);
            gfx.InterpolationMode = ratio != 1.0f ? InterpolationMode.HighQualityBicubic : InterpolationMode.Default;
            gfx.DrawImage(input, pos.X, pos.Y, size.Width, size.Height);

            return thumb;
        }

        private void GenerateThumbnailBackground()
        {
            var thumbWidth = Settings.Default.ThumbnailWidth;
            var thumbHeight = Settings.Default.ThumbnailHeight;
            var thumb = new Bitmap(thumbWidth, thumbHeight, PixelFormat.Format24bppRgb);
            var gfx = Graphics.FromImage(thumb);

            // Grid
            var xCount = Settings.Default.ThumbnailWidth / 16 + 1;
            var yCount = Settings.Default.ThumbnailHeight / 16 + 1;

            gfx.FillRectangle(new SolidBrush(Settings.Default.GridColor1), 0, 0, thumbWidth, thumbHeight);
            for (var i = 0; i < xCount; i++)
                for (var j = 0; j < yCount; j++)
                    if ((i + j) % 2 != 1)
                        gfx.FillRectangle(new SolidBrush(Settings.Default.GridColor2), i * 16, j * 16, 16, 16);

            _thumbnailBackground = thumb;
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

                // Properties
                tsbExtendedProperties.Enabled = _fileOpen && _imageAdapter.FileHasExtendedProperties;
            }

            // Batch Import/Export
            batchScanSubdirectoriesToolStripMenuItem.Text = Settings.Default.BatchScanSubdirectories ? "Scan Sub-directories" : "Don't Scan Sub-directories";
            batchScanSubdirectoriesToolStripMenuItem.Image = Settings.Default.BatchScanSubdirectories ? Resources.menu_scan_subdirectories_on : Resources.menu_scan_subdirectories_off;
            tsbBatchScanSubdirectories.Text = batchScanSubdirectoriesToolStripMenuItem.Text;
            tsbBatchScanSubdirectories.Image = batchScanSubdirectoriesToolStripMenuItem.Image;

            tsbImageBorderStyle.Enabled = _fileOpen;
            tsbImageBorderColor.Enabled = _fileOpen;
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
                var path = fbd.SelectedPath;
                var count = 0;
                var exported = 0;
                var errors = 0;

                if (Directory.Exists(path))
                {
                    var types = _imageAdapters.Select(x => x.Extension.ToLower()).Select(y => y.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)).SelectMany(z => z).Distinct().ToList();

                    var files = new List<string>();
                    foreach (var type in types)
                        files.AddRange(Directory.GetFiles(path, type, Settings.Default.BatchScanSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));

                    Parallel.ForEach(files, file => batchExportPNGTask(file, ref count, ref exported, ref errors));

                    MessageBox.Show($"Batch export completed {((errors > 0) ? "with " + errors + " errors" : "successfully")}. " + count + " texture(s) succesfully exported to " + exported + " images.", "Batch Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            Settings.Default.LastBatchDirectory = fbd.SelectedPath;
            Settings.Default.Save();
        }

        private void batchExportPNGTask(string file, ref Int32 count, ref Int32 exported, ref Int32 errors)
        {
            if (File.Exists(file))
                try {
                    var fi = new FileInfo(file);
                    var currentAdapter = SelectImageAdapter(file, true);

                    if (currentAdapter != null) {
                        currentAdapter.Load(file);
                        for (var i = 0; i < currentAdapter.Bitmaps.Count; i++) {
                            currentAdapter.Bitmaps[i].Bitmap.Save(fi.FullName + "." + i.ToString("00") + ".png");
                            Interlocked.Increment(ref exported);
                        }
                        Interlocked.Increment(ref count);
                    }
                } catch (Exception) {
                    Interlocked.Increment(ref errors);
                }
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
                var path = fbd.SelectedPath;
                var fileCount = 0;
                var importCount = 0;
                var errors = false;

                if (Directory.Exists(path))
                {
                    var types = _imageAdapters.Select(x => x.Extension.ToLower()).Select(y => y.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)).SelectMany(z => z).Distinct().ToList();

                    var files = new List<string>();
                    foreach (var type in types)
                        files.AddRange(Directory.GetFiles(path, type, Settings.Default.BatchScanSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));

                    foreach (var file in files)
                        if (File.Exists(file))
                            try
                            {
                                var fi = new FileInfo(file);
                                var currentAdapter = SelectImageAdapter(file, true);

                                if (currentAdapter != null && currentAdapter.CanSave)
                                {
                                    currentAdapter.Load(file);
                                    for (var i = 0; i < currentAdapter.Bitmaps.Count; i++)
                                    {
                                        var targetName = fi.FullName + "." + i.ToString("00") + ".png";
                                        if (!File.Exists(targetName)) continue;
                                        currentAdapter.Bitmaps[i].Bitmap = new Bitmap(targetName);
                                    }
                                    currentAdapter.Save();
                                    importCount++;
                                }

                                fileCount++;
                            }
                            catch (Exception)
                            {
                                errors = true;
                            }

                    MessageBox.Show($"Batch import completed {(errors ? "with errors" : "successfully")}. " + importCount + " of " + fileCount + " files succesfully imported.", "Batch Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        // Image Box Colors
        private void tsbGridColor1_Click(object sender, EventArgs e)
        {
            clrDialog.Color = imbPreview.GridColor;
            if (clrDialog.ShowDialog() != DialogResult.OK) return;

            imbPreview.GridColor = clrDialog.Color;
            Settings.Default.GridColor1 = clrDialog.Color;
            Settings.Default.Save();
            UpdatePreview();
            GenerateThumbnailBackground();
            UpdateImageList();
            if (_fileOpen)
                treBitmaps.SelectedNode = treBitmaps.Nodes[_selectedImageIndex];
        }

        private void tsbGridColor2_Click(object sender, EventArgs e)
        {
            clrDialog.Color = imbPreview.GridColorAlternate;
            if (clrDialog.ShowDialog() != DialogResult.OK) return;

            imbPreview.GridColorAlternate = clrDialog.Color;
            Settings.Default.GridColor2 = clrDialog.Color;
            Settings.Default.Save();
            UpdatePreview();
            GenerateThumbnailBackground();
            UpdateImageList();
            if (_fileOpen)
                treBitmaps.SelectedNode = treBitmaps.Nodes[_selectedImageIndex];
        }

        private void tsbImageBorderStyle_Click(object sender, EventArgs e)
        {
            var tsb = (ToolStripMenuItem)sender;
            var style = (ImageBoxBorderStyle)Enum.Parse(typeof(ImageBoxBorderStyle), tsb.Tag.ToString());

            imbPreview.ImageBorderStyle = style;
            Settings.Default.ImageBorderStyle = style;
            Settings.Default.Save();
            UpdatePreview();
        }

        private void tsbImageBorderColor_Click(object sender, EventArgs e)
        {
            clrDialog.Color = imbPreview.ImageBorderColor;
            if (clrDialog.ShowDialog() != DialogResult.OK) return;

            imbPreview.ImageBorderColor = clrDialog.Color;
            Settings.Default.ImageBorderColor = clrDialog.Color;
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
        private void imbPreview_MouseEnter(object sender, EventArgs e)
        {
            imbPreview.Focus();
        }

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

        private void treBitmaps_MouseEnter(object sender, EventArgs e)
        {
            treBitmaps.Focus();
        }

        private void cmsPreviewCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetImage(imbPreview.Image);
        }

        // Info Controls
        private void treBitmaps_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _selectedImageIndex = treBitmaps.SelectedNode.Index;
            UpdatePreview();
        }

        // Properties
        private void tsbExtendedProperties_Click(object sender, EventArgs e)
        {
            if (_imageAdapter.ShowProperties(Resources.kukkii))
            {
                _hasChanges = true;
                UpdateForm();
            }
        }
    }
}
