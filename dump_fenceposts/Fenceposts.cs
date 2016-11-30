using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using dump_fenceposts.Properties;
using file_kup;
using KuriimuContract;
using System.Xml;

namespace dump_fenceposts
{
	public partial class Fenceposts : Form
	{
		private FileInfo _file = null;
		private bool _fileOpen = false;
		private bool _hasChanges = false;

		KUP _kup = null;
		Dictionary<long, long> _pointers = null;

		private BackgroundWorker _worker = new BackgroundWorker();

		public Fenceposts()
		{
			InitializeComponent();
		}

		private void Fenceposts_Load(object sender, EventArgs e)
		{
			this.Text = Settings.Default.PluginName;
			this.Icon = Resources.fenceposts;

			// Configure workers
			_worker.DoWork += new DoWorkEventHandler(worker_DoWork);
			_worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
			_worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
			_worker.WorkerReportsProgress = true;
			_worker.WorkerSupportsCancellation = true;

			UpdateForm();
		}

		private void Fenceposts_FormClosing(object sender, FormClosingEventArgs e)
		{
			_worker.CancelAsync();
		}

		private void newToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ConfirmNewFile();
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ConfirmOpenFile();
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFile();
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFile(true);
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void ConfirmNewFile()
		{
			DialogResult dr = DialogResult.No;

			if (_fileOpen && _hasChanges)
				dr = MessageBox.Show("You have unsaved changes in " + FileName() + ". Save changes before creating a new file?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

			switch (dr)
			{
				case DialogResult.Yes:
					dr = SaveFile();
					if (dr == DialogResult.OK) NewFile();
					break;
				case DialogResult.No:
					NewFile();
					break;
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

		private void NewFile()
		{
			_file = null;
			_fileOpen = true;
			_kup = new KUP();
			LoadForm();
			_hasChanges = false;
			UpdateForm();
		}

		private void OpenFile(string filename = "")
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "Kuriimu Archive (*.kup)|*.kup";
			DialogResult dr = DialogResult.OK;

			if (filename == string.Empty)
			{
				dr = ofd.ShowDialog();
				filename = ofd.FileName;
			}

			if (dr == DialogResult.OK)
			{
				try
				{
					_file = new FileInfo(filename);
					_fileOpen = true;
					_kup = KUP.Load(filename);
					LoadForm();
					_hasChanges = false;
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK);
					_fileOpen = false;
					_hasChanges = false;
				}

				UpdateForm();
			}
		}

		private DialogResult SaveFile(bool saveAs = false)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filter = "Kuriimu Archive (*.kup)|*.kup";
			DialogResult dr = DialogResult.OK;

			if (_file == null || saveAs)
			{
				dr = sfd.ShowDialog();
			}

			if ((_file == null || saveAs) && dr == DialogResult.OK)
			{
				_file = new FileInfo(sfd.FileName);
			}

			if (dr == DialogResult.OK)
			{
				_kup.Save(_file.FullName);
				_hasChanges = false;
				UpdateForm();
			}

			return dr;
		}

		private void LoadForm()
		{
			lstPointerTables.Items.Clear();
			foreach (Bound bound in _kup.PointerTables)
				lstPointerTables.Items.Add(bound);
			if (lstPointerTables.Items.Count > 0)
				lstPointerTables.SelectedIndex = 0;

			lstDumpingBounds.Items.Clear();
			foreach (Bound bound in _kup.DumpingBounds)
				lstDumpingBounds.Items.Add(bound);
			if (lstDumpingBounds.Items.Count > 0)
				lstDumpingBounds.SelectedIndex = 0;

			txtFilename.Text = _kup.File;
			txtRamOffset.Text = _kup.RamOffset;
		}

		// Utilities
		private void UpdateForm()
		{
			this.Text = Settings.Default.PluginName + (FileName() != string.Empty ? " - " + FileName() : string.Empty) + (_hasChanges ? "*" : string.Empty);

			bool pointerTableSelected = lstPointerTables.SelectedIndex >= 0;
			tsbPointerTableAdd.Enabled = _fileOpen;
			tsbPointerTableProperties.Enabled = pointerTableSelected;
			tsbPointerTableDelete.Enabled = pointerTableSelected;
			lstPointerTables.Enabled = _fileOpen;

			bool dumpingBoundSelected = lstDumpingBounds.SelectedIndex >= 0;
			tsbDumpingBoundAdd.Enabled = _fileOpen;
			tsbDumpingBoundProperties.Enabled = dumpingBoundSelected;
			tsbDumpingBoundDelete.Enabled = dumpingBoundSelected;
			lstDumpingBounds.Enabled = _fileOpen;

			txtFilename.Enabled = _fileOpen;
			btnBrowse.Enabled = _fileOpen;
			txtRamOffset.Enabled = _fileOpen;
			btnDump.Enabled = (lstPointerTables.Items.Count > 0 && lstDumpingBounds.Items.Count > 0);
			txtOutput.Enabled = _fileOpen;
			lstStatus.Enabled = _fileOpen;
		}

		private string FileName()
		{
			string result = "Untitiled";

			if (_file != null && _fileOpen)
				result = _file.Name;

			return result;
		}

		// Functions
		private void btnBrowse_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "All files (*.*)|*.*";

			if (ofd.ShowDialog() == DialogResult.OK)
				txtFilename.Text = ofd.FileName;
		}

		private void btnDump_Click(object sender, EventArgs e)
		{
			if (File.Exists(txtFilename.Text))
			{
				mnuMain.Enabled = false;
				tlsPointerTables.Enabled = false;
				tlsDumpingBounds.Enabled = false;
				lstPointerTables.Enabled = false;
				lstDumpingBounds.Enabled = false;
				txtFilename.Enabled = false;
				btnBrowse.Enabled = false;
				btnDump.Enabled = false;

				lstStatus.Items.Clear();
				_kup.Entries.Clear();
				_worker.RunWorkerAsync();
			}
		}

		private void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			FileStream fs = new FileStream(txtFilename.Text, FileMode.Open, FileAccess.Read, FileShare.Read);
			BinaryReaderX br = new BinaryReaderX(fs, ByteOrder.LittleEndian);

			// Pointer Tables
			_pointers = new Dictionary<long, long>();

			_worker.ReportProgress(0, "STATUS|Dumping pointers...");
			foreach (Bound bound in _kup.PointerTables)
			{
				br.BaseStream.Seek(bound.StartLong, SeekOrigin.Begin);

				while (br.BaseStream.Position < bound.EndLong)
				{
					uint offset = br.ReadUInt32() - _kup.RamOffsetUInt;
					if (offset < br.BaseStream.Length - 3)
						_pointers.Add(br.BaseStream.Position, offset);

					_worker.ReportProgress((int)(((double)br.BaseStream.Position - bound.StartLong) / ((double)bound.EndLong - bound.StartLong) * 100), "BOTTOM");
				}
			}
			_worker.ReportProgress(0, "STATUS|Found " + _pointers.Count + " pointers...");

			_worker.ReportProgress(0, "STATUS|Dumping strings...");
			for (int i = 0; i < _kup.DumpingBounds.Count; i++)
			{
				br.BaseStream.Seek(_kup.DumpingBounds[i].StartLong, SeekOrigin.Begin);

				_worker.ReportProgress((int)((double)i / (double)_kup.DumpingBounds.Count * 100), "BOTTOM");

				List<byte> result = new List<byte>();
				long offset = br.BaseStream.Position;

				while (br.BaseStream.Position < _kup.DumpingBounds[i].EndLong)
				{
					byte[] unichar = br.ReadBytes(2);
					result.AddRange(unichar);

					if (_pointers.Values.Contains(br.BaseStream.Position))
					{
						if (result.Count >= 2)
						{
							if (result[result.Count - 1] == 0x00 && result[result.Count - 2] == 0x00)
							{
								result.RemoveAt(result.Count - 1);
								result.RemoveAt(result.Count - 1);
							}

							Entry entry = new Entry(_kup.Encoding);
							entry.OffsetLong = offset;
							foreach (long key in _pointers.Keys)
								if (_pointers[key] == offset)
									entry.Pointers.Add(new Pointer(key));
							entry.OriginalText = result.ToArray();
							entry.EditedText = entry.OriginalText;
							entry.Relocatable = true;
							entry.Name = Regex.Match(_kup.Encoding.GetString(entry.OriginalText), @"\w+", RegexOptions.IgnoreCase).Value;
							_kup.Entries.Add(entry);

							_worker.ReportProgress(0, "TEXT|" + _kup.Encoding.GetString(result.ToArray()).Replace("\0", @"<null>") + "\r\n\r\n");
						}

						offset = br.BaseStream.Position;
						result.Clear();
					}

					_worker.ReportProgress((int)(((double)br.BaseStream.Position - _kup.DumpingBounds[i].StartLong) / ((double)_kup.DumpingBounds[i].EndLong - _kup.DumpingBounds[i].StartLong) * 100), "TOP");
				}
			}
			_worker.ReportProgress(0, "STATUS|Dumped " + _kup.Entries.Count + " strings...");

			br.Close();
		}

		private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			string[] items = e.UserState.ToString().Split('|');

			switch(items[0])
			{
				case "BOTTOM":
					prgBottom.Value = e.ProgressPercentage;
					break;
				case "TOP":
					prgTop.Value = e.ProgressPercentage;
					break;
				case "STATUS":
					lstStatus.Items.Add(items[1]);
					break;
				case "TEXT":
					txtOutput.AppendText(items[1]);
					break;
			}
		}

		private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			prgTop.Value = 100;
			prgBottom.Value = 100;
			mnuMain.Enabled = true;
			tlsPointerTables.Enabled = true;
			tlsDumpingBounds.Enabled = true;
			UpdateForm();
		}

		// Toolbar
		private void tsbPointerTableAdd_Click(object sender, EventArgs e)
		{
			Bound bound = new Bound();
			frmBound frm = new frmBound(bound);
			frm.StartPosition = FormStartPosition.CenterParent;

			if (frm.ShowDialog() == DialogResult.OK)
			{
				_kup.PointerTables.Add(bound);
				_hasChanges = true;
				LoadForm();
			}
		}

		private void tsbPointerTableProperties_Click(object sender, EventArgs e)
		{
			frmBound frm = new frmBound((Bound)lstPointerTables.SelectedItem);
			frm.StartPosition = FormStartPosition.CenterParent;

			if (frm.ShowDialog() == DialogResult.OK)
			{
				_hasChanges = true;
				LoadForm();
			}
		}

		private void tsbPointerTableDelete_Click(object sender, EventArgs e)
		{

		}

		private void tsbDumpingBoundAdd_Click(object sender, EventArgs e)
		{
			Bound bound = new Bound();
			frmBound frm = new frmBound(bound);
			frm.StartPosition = FormStartPosition.CenterParent;

			if (frm.ShowDialog() == DialogResult.OK)
			{
				_kup.DumpingBounds.Add(bound);
				_hasChanges = true;
				LoadForm();
			}
		}

		private void tsbDumpingBoundProperties_Click(object sender, EventArgs e)
		{
			frmBound frm = new frmBound((Bound)lstDumpingBounds.SelectedItem);
			frm.StartPosition = FormStartPosition.CenterParent;

			if (frm.ShowDialog() == DialogResult.OK)
			{
				_hasChanges = true;
				LoadForm();
			}
		}

		private void tsbDumpingBoundDelete_Click(object sender, EventArgs e)
		{

		}

		// List
		private void lstPointerTables_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateForm();
		}

		private void lstDumpingBounds_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateForm();
		}

		// Change Detection
		private void txtFilename_TextChanged(object sender, EventArgs e)
		{
			_kup.File = txtFilename.Text;
			_hasChanges = true;
			UpdateForm();
		}

		private void txtRamOffset_TextChanged(object sender, EventArgs e)
		{
			_kup.RamOffset = txtRamOffset.Text;
			_hasChanges = true;
			UpdateForm();
		}

	}
}