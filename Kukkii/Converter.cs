using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Kukkii.Properties;
using KuriimuContract;

namespace Kukkii
{
	public partial class frmConverter : Form
	{
		private IImageAdapter _imageAdapter = null;
		private bool _fileOpen = false;
		private bool _hasChanges = false;

		private List<IImageAdapter> _imageAdapters = null;

		public frmConverter(string[] args)
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
			UpdateForm();
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

		private void exportToPNGToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ExportToPNG();
		}
		private void tsbExportToPNG_Click(object sender, EventArgs e)
		{
			ExportToPNG();
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
				dr = MessageBox.Show("You have unsaved changes in " + FileName() + ". Save changes before opening another file?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

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
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.InitialDirectory = Settings.Default.LastDirectory;

			// Supported Types
			ofd.Filter = Tools.LoadImageFilters(_imageAdapters);

			DialogResult dr = DialogResult.OK;

			if (filename == string.Empty)
				dr = ofd.ShowDialog();

			if (dr == DialogResult.OK)
			{
				if (filename == string.Empty)
					filename = ofd.FileName;

				IImageAdapter _tempAdapter = SelectImageAdapter(filename);

				try
				{
					if (_tempAdapter != null && _tempAdapter.Load(filename) == LoadResult.Success)
					{
						_imageAdapter = _tempAdapter;
						_fileOpen = true;
						_hasChanges = false;

						UpdatePreview();
						UpdateForm();
					}

					Settings.Default.LastDirectory = new FileInfo(filename).DirectoryName;
					Settings.Default.Save();
				}
				catch (Exception ex)
				{
					if (_tempAdapter != null)
						MessageBox.Show(this, ex.ToString(), _tempAdapter.Name + " - " + _tempAdapter.Description + " Adapter", MessageBoxButtons.OK, MessageBoxIcon.Error);
					else
						MessageBox.Show(this, ex.ToString(), "Supported Format Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private DialogResult SaveFile(bool saveAs = false)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			DialogResult dr = DialogResult.OK;

			sfd.Title = "Save as " + _imageAdapter.Description;
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

			if (dr == DialogResult.OK)
			{
				_imageAdapter.Save(_imageAdapter.FileInfo.FullName);
				_hasChanges = false;
				UpdateForm();
			}

			return dr;
		}

		private IImageAdapter SelectImageAdapter(string filename, bool batchMode = false)
		{
			IImageAdapter result = null;

			// first look for adapters whose extension matches that of our filename
			List<IImageAdapter> matchingAdapters = _imageAdapters.Where(adapter => adapter.Extension.Split(';').Any(s => filename.ToLower().EndsWith(s.Substring(1).ToLower()))).ToList();

			result = matchingAdapters.FirstOrDefault(adapter => adapter.Identify(filename));

			// if none of them match, then try all other adapters
			if (result == null)
				result = _imageAdapters.Except(matchingAdapters).FirstOrDefault(adapter => adapter.Identify(filename));

			if (result == null && !batchMode)
				MessageBox.Show("None of the installed plugins are able to open the file.", "Unsupported Format", MessageBoxButtons.OK, MessageBoxIcon.Information);

			return result;
		}

		private void ExportToPNG()
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Title = "Export to PNG...";
			sfd.InitialDirectory = Settings.Default.LastDirectory;
			sfd.FileName = _imageAdapter.FileInfo.Name + ".png";
			sfd.Filter = "Portable Network Graphics (*.png)|*.png";
			sfd.AddExtension = true;

			if (sfd.ShowDialog() == DialogResult.OK)
				imbPreview.Image.Save(sfd.FileName, ImageFormat.Png);
		}

		private void ImportFromPNG()
		{

		}

		private void Import(string filename)
		{
			try
			{
				var bmp = (Bitmap)Image.FromFile(filename);
				_imageAdapter.Bitmap = bmp;
				UpdatePreview();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void UpdatePreview()
		{
			imbPreview.Image = _imageAdapter.Bitmap;
			imbPreview.Zoom = 100;
		}

		private void UpdateForm()
		{
			Text = Settings.Default.ApplicationName + " " + Settings.Default.ApplicationVersion + (FileName() != string.Empty ? " - " + FileName() : string.Empty) + (_hasChanges ? "*" : string.Empty);

			if (_imageAdapter != null)
			{
				// Menu
				saveToolStripMenuItem.Enabled = _fileOpen && _imageAdapter.CanSave;
				tsbSave.Enabled = _fileOpen && _imageAdapter.CanSave;
				saveAsToolStripMenuItem.Enabled = _fileOpen && _imageAdapter.CanSave;
				tsbSaveAs.Enabled = _fileOpen && _imageAdapter.CanSave;

				// Toolbar
				exportToPNGToolStripMenuItem.Enabled = _fileOpen;
				tsbExportToPNG.Enabled = _fileOpen;
				//entryPropertiesToolStripMenuItem.Enabled = itemSelected && _fileAdapter.EntriesHaveExtendedProperties;
				//tsbEntryProperties.Enabled = itemSelected && _fileAdapter.EntriesHaveExtendedProperties;
			}
		}

		private string FileName()
		{
			return _imageAdapter == null || _imageAdapter.FileInfo == null ? string.Empty : _imageAdapter.FileInfo.Name;
		}

		// Toolbar Features
		private void tsbBatchExport_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			fbd.Description = "Select the source directory containing image files...";
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
						files.AddRange(Directory.GetFiles(path, type));

					foreach (string file in files)
					{
						if (File.Exists(file))
						{
							FileInfo fi = new FileInfo(file);
							IImageAdapter currentAdapter = SelectImageAdapter(file, true);

							if (currentAdapter != null)
							{
								currentAdapter.Load(file);
								currentAdapter.Bitmap.Save(fi.FullName + ".png");
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

		// Image Box
		private void imbPreview_Zoomed(object sender, Cyotek.Windows.Forms.ImageBoxZoomEventArgs e)
		{
			tslZoom.Text = "Zoom: " + imbPreview.Zoom + "%";
		}

		private void imbPreview_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space)
			{
				imbPreview.SelectionMode = Cyotek.Windows.Forms.ImageBoxSelectionMode.None;
				imbPreview.Cursor = Cursors.SizeAll;
				tslTool.Text = "Tool: Pan";
			}
		}

		private void imbPreview_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space)
			{
				imbPreview.SelectionMode = Cyotek.Windows.Forms.ImageBoxSelectionMode.Zoom;
				imbPreview.Cursor = Cursors.Default;
				tslTool.Text = "Tool: Zoom";
			}
		}
	}
}