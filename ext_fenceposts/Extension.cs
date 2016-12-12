using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ext_fenceposts.Properties;
using file_kup;
using KuriimuContract;
using System.Xml;

namespace ext_fenceposts
{
	public partial class Extension : Form
	{
		private FileInfo _file = null;
		private IGameHandler _gameHandler = null;
		private bool _fileOpen = false;
		private bool _hasChanges = false;

		private KUP _kup = null;
		private Dictionary<long, long> _pointers = null;
		private Dictionary<string, IGameHandler> _gameHandlers = null;

		private BackgroundWorker _workerDumper = new BackgroundWorker();
		private BackgroundWorker _workerInjector = new BackgroundWorker();

		public Extension()
		{
			InitializeComponent();
		}

		private void Fenceposts_Load(object sender, EventArgs e)
		{
			this.Text = Settings.Default.PluginName;
			this.Icon = Resources.fenceposts;

			// Load Plugins
			Console.WriteLine("Loading plugins...");

			tsbGameSelect.DropDownItems.Clear();
			ToolStripMenuItem tsiNoGame = new ToolStripMenuItem("No Game", Resources.game_none, tsbGameSelect_SelectedIndexChanged);
			tsbGameSelect.DropDownItems.Add(tsiNoGame);
			tsbGameSelect.Text = tsiNoGame.Text;
			tsbGameSelect.Image = tsiNoGame.Image;

			_gameHandlers = new Dictionary<string, IGameHandler>();
			foreach (IGameHandler gameHandler in PluginLoader<IGameHandler>.LoadPlugins(Settings.Default.PluginDirectory, "game*.dll"))
			{
				_gameHandlers.Add(gameHandler.Name, gameHandler);

				ToolStripMenuItem tsiHandler = new ToolStripMenuItem(gameHandler.Name, gameHandler.Icon, tsbGameSelect_SelectedIndexChanged);
				tsbGameSelect.DropDownItems.Add(tsiHandler);
			}

			// Configure workers
			_workerDumper.DoWork += new DoWorkEventHandler(workerDumper_DoWork);
			_workerDumper.ProgressChanged += new ProgressChangedEventHandler(workerDumper_ProgressChanged);
			_workerDumper.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerDumper_RunWorkerCompleted);
			_workerDumper.WorkerReportsProgress = true;
			_workerDumper.WorkerSupportsCancellation = true;

			_workerInjector.DoWork += new DoWorkEventHandler(workerInjector_DoWork);
			_workerInjector.ProgressChanged += new ProgressChangedEventHandler(workerInjector_ProgressChanged);
			_workerInjector.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerInjector_RunWorkerCompleted);
			_workerInjector.WorkerReportsProgress = true;
			_workerInjector.WorkerSupportsCancellation = true;

			UpdateForm();
		}

		private void Fenceposts_FormClosing(object sender, FormClosingEventArgs e)
		{
			_workerDumper.CancelAsync();
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

		private void tsbNew_Click(object sender, EventArgs e)
		{
			ConfirmNewFile();
		}

		private void tsbOpen_Click(object sender, EventArgs e)
		{
			ConfirmOpenFile();
		}

		private void tsbSave_Click(object sender, EventArgs e)
		{
			SaveFile();
		}

		private void tsbSaveAs_Click(object sender, EventArgs e)
		{
			SaveFile(true);
		}

		private void tsbGameSelect_SelectedIndexChanged(object sender, EventArgs e)
		{
			ToolStripItem tsi = (ToolStripItem)sender;

			if (tsi.Text == "No Game")
				_gameHandler = null;
			else
				_gameHandler = _gameHandlers[((ToolStripItem)sender).Text];

			tsbGameSelect.Text = tsi.Text;
			tsbGameSelect.Image = tsi.Image;
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

			lstStringBounds.Items.Clear();
			foreach (Bound bound in _kup.StringBounds)
				lstStringBounds.Items.Add(bound);
			if (lstStringBounds.Items.Count > 0)
				lstStringBounds.SelectedIndex = 0;

			txtFilename.Text = _kup.File;
			txtRamOffset.Text = _kup.RamOffset;
			chkCleanDump.Checked = Settings.Default.CleanDump;
		}

		// Utilities
		private void UpdateForm()
		{
			this.Text = Settings.Default.PluginName + (FileName() != string.Empty ? " - " + FileName() : string.Empty) + (_hasChanges ? "*" : string.Empty);

			saveToolStripMenuItem.Enabled = _fileOpen;
			saveAsToolStripMenuItem.Enabled = _fileOpen;
			tsbSave.Enabled = _fileOpen;
			tsbSaveAs.Enabled = _fileOpen;

			splConfigure.Enabled = _fileOpen;
			splBounds.Enabled = _fileOpen;

			tsbGameSelect.Enabled = _fileOpen;

			bool pointerTableSelected = lstPointerTables.SelectedIndex >= 0;
			tsbPointerTableAdd.Enabled = _fileOpen;
			tsbPointerTableProperties.Enabled = pointerTableSelected;
			tsbPointerTableDelete.Enabled = pointerTableSelected;
			lstPointerTables.Enabled = _fileOpen;

			bool dumpingBoundSelected = lstStringBounds.SelectedIndex >= 0;
			tsbStringBoundAdd.Enabled = _fileOpen;
			tsbStringBoundProperties.Enabled = dumpingBoundSelected;
			tsbStringBoundDelete.Enabled = dumpingBoundSelected;
			lstStringBounds.Enabled = _fileOpen;

			txtFilename.Enabled = _fileOpen;
			btnBrowse.Enabled = _fileOpen;
			txtRamOffset.Enabled = _fileOpen;
			chkCleanDump.Enabled = _fileOpen;
			btnDump.Enabled = (lstPointerTables.Items.Count > 0 && lstStringBounds.Items.Count > 0);
			btnInject.Enabled = lstStringBounds.Items.Count > 0;
			lstStatus.Enabled = _fileOpen;
		}

		private string FileName()
		{
			string result = "Untitled";

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
				tlsMain.Enabled = false;
				tlsPointerTables.Enabled = false;
				tlsStringBounds.Enabled = false;
				lstPointerTables.Enabled = false;
				lstStringBounds.Enabled = false;
				txtFilename.Enabled = false;
				btnBrowse.Enabled = false;
				btnDump.Enabled = false;
				btnInject.Enabled = false;

				prgTop.Value = 0;
				prgBottom.Value = 0;
				lstStatus.Items.Clear();
				if (chkCleanDump.Checked)
					_kup.Entries.Clear();
				_workerDumper.RunWorkerAsync();
			}
		}

		private void btnInject_Click(object sender, EventArgs e)
		{
			if (File.Exists(txtFilename.Text))
			{
				tlsMain.Enabled = false;
				tlsPointerTables.Enabled = false;
				tlsStringBounds.Enabled = false;
				lstPointerTables.Enabled = false;
				lstStringBounds.Enabled = false;
				txtFilename.Enabled = false;
				btnBrowse.Enabled = false;
				btnDump.Enabled = false;
				btnInject.Enabled = false;

				prgTop.Value = 0;
				prgBottom.Value = 0;
				lstStatus.Items.Clear();
				_workerInjector.RunWorkerAsync();
			}
		}

		// Dumper
		private void workerDumper_DoWork(object sender, DoWorkEventArgs e)
		{
			FileStream fs = new FileStream(txtFilename.Text, FileMode.Open, FileAccess.Read, FileShare.Read);
			BinaryReaderX br = new BinaryReaderX(fs, ByteOrder.LittleEndian);

			// Pointer Tables
			_pointers = new Dictionary<long, long>();

			_workerDumper.ReportProgress(0, "STATUS|Dumping pointers...");
			foreach (Bound pointerBound in _kup.PointerTables)
			{
				long end = Math.Max(4, pointerBound.EndLong);

				if (end == 4)
					end += pointerBound.StartLong;

				br.BaseStream.Seek(pointerBound.StartLong, SeekOrigin.Begin);
				while (br.BaseStream.Position < end)
				{
					uint offset = br.ReadUInt32() - _kup.RamOffsetUInt;
					if (offset < br.BaseStream.Length)
						_pointers.Add(br.BaseStream.Position - sizeof(uint), offset); // AAAHAAAAARRRRRRGGRGGGGHHHHHHH

					_workerDumper.ReportProgress((int)(((double)br.BaseStream.Position - pointerBound.StartLong) / ((double)pointerBound.EndLong - pointerBound.StartLong) * 100), "BOTTOM");
				}
			}
			_workerDumper.ReportProgress(0, "STATUS|Found " + _pointers.Count + " pointers...");

			_workerDumper.ReportProgress(0, "STATUS|Dumping strings...");
			for (int i = 0; i < _kup.StringBounds.Count; i++)
			{
				Bound stringBound = _kup.StringBounds[i];

				_workerDumper.ReportProgress((int)(((double)i / (double)_kup.StringBounds.Count) * 100), "BOTTOM");

				if (stringBound.Dumpable)
				{
					List<byte> result = new List<byte>();
					long offset = stringBound.StartLong;

					br.BaseStream.Seek(stringBound.StartLong, SeekOrigin.Begin);
					while (br.BaseStream.Position < stringBound.EndLong)
					{
						byte[] unichar = br.ReadBytes(2);
						result.AddRange(unichar);

						if (stringBound.Injectable)
						{
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

									// Merging
									bool matched = false;
									foreach (Entry ent in _kup.Entries)
										if (ent.Offset == entry.Offset)
										{
											entry = ent;
											matched = true;
											break;
										}

									if (matched)
									{
										foreach (long key in _pointers.Keys)
											if (_pointers[key] == offset && !entry.Pointers.Contains(new Pointer(key)))
												entry.Pointers.Add(new Pointer(key));
										if (Array.Equals(entry.EditedText, entry.OriginalText))
											entry.EditedText = result.ToArray();
										entry.OriginalText = result.ToArray();
									}
									else
									{
										foreach (long key in _pointers.Keys)
											if (_pointers[key] == offset)
												entry.Pointers.Add(new Pointer(key));
										entry.OriginalText = result.ToArray();
										entry.EditedText = result.ToArray();
										entry.Relocatable = stringBound.Injectable;
										entry.Name = Regex.Match(_kup.Encoding.GetString(entry.OriginalText), @"\w+", RegexOptions.IgnoreCase).Value;
										_kup.Entries.Add(entry);
									}

									string str = entry.GetEditedString();

									if (_gameHandler != null)
										str = _gameHandler.GetString(entry.EditedText, entry.Encoding).Replace("\0", "<null>").Replace("\n", "\r\n");
									else
										str = entry.GetEditedString().Replace("\0", "<null>").Replace("\n", "\r\n");

									if (Regex.Matches(str, "<null>").Count > 0)
										_workerDumper.ReportProgress(0, "STATUS|Found a potentially broken string at " + entry.Offset + ": " + entry.Name + "|" + entry.Offset);
								}

								offset = br.BaseStream.Position;
								result.Clear();
							}
						}
						else
						{
							if (br.BaseStream.Position == stringBound.EndLong)
							{
								if (result.Count >= 2)
								{
									if (result[result.Count - 1] == 0x00 && result[result.Count - 2] == 0x00)
									{
										result.RemoveAt(result.Count - 1);
										result.RemoveAt(result.Count - 1);
									}

									Entry entry = new Entry(_kup.Encoding);
									entry.OffsetLong = stringBound.StartLong;

									// Merging
									bool matched = false;
									foreach (Entry ent in _kup.Entries)
										if (ent.Offset == entry.Offset)
										{
											entry = ent;
											matched = true;
											break;
										}

									if (matched)
									{
										if (Array.Equals(entry.EditedText, entry.OriginalText))
											entry.EditedText = result.ToArray();
										entry.OriginalText = result.ToArray();
									}
									else
									{
										entry.OriginalText = result.ToArray();
										entry.EditedText = entry.OriginalText;
										entry.Relocatable = stringBound.Injectable;
										entry.Name = Regex.Match(_kup.Encoding.GetString(entry.OriginalText), @"\w+", RegexOptions.IgnoreCase).Value;
										entry.MaxLength = (int)(stringBound.EndLong - stringBound.StartLong - (_kup.Encoding.IsSingleByte ? 1 : 2));
										_kup.Entries.Add(entry);
									}
								}

								result.Clear();
							}
						}

						_workerDumper.ReportProgress((int)(((double)br.BaseStream.Position - stringBound.StartLong) / ((double)stringBound.EndLong - stringBound.StartLong) * 100), "TOP");
					}
				}
			}
			_workerDumper.ReportProgress(0, "STATUS|Dumped " + _kup.Entries.Count + " strings...");

			br.Close();
		}

		private void workerDumper_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			string[] items = e.UserState.ToString().Split('|');

			switch(items[0])
			{
				case "TOP":
					prgTop.Value = Math.Max(Math.Min(e.ProgressPercentage, 100), 0);
					break;
				case "BOTTOM":
					prgBottom.Value = Math.Max(Math.Min(e.ProgressPercentage, 100), 0);
					break;
				case "STATUS":
					if (items.Length == 3)
						lstStatus.Items.Add(new ListItem(items[1], items[2]));
					else
						lstStatus.Items.Add(items[1]);
					if(lstStatus.Items.Count > 0)
						lstStatus.SelectedIndex = lstStatus.Items.Count - 1;
					break;
			}
		}

		private void workerDumper_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			prgTop.Value = 100;
			prgBottom.Value = 100;
			tlsMain.Enabled = true;
			tlsPointerTables.Enabled = true;
			tlsStringBounds.Enabled = true;
			_hasChanges = true;
			UpdateForm();
		}

		// Injector
		private int SortEntriesForInjection(Entry lhs, Entry rhs)
		{
			int result = lhs.EditedText.Length.CompareTo(rhs.EditedText.Length);
			if (result == 0)
			{
				string myString = lhs.Encoding.GetString(lhs.EditedText);
				string theirString = rhs.Encoding.GetString(rhs.EditedText);
				result = myString.CompareTo(theirString);
			}
			return result;
		}

		private void workerInjector_DoWork(object sender, DoWorkEventArgs e)
		{
			File.Copy(txtFilename.Text, txtFilename.Text + ".injected", true);
			FileInfo codeFile = new FileInfo(txtFilename.Text + ".injected");

			List<Entry> injectedEntries = new List<Entry>();

			// Sort Entries
			_kup.Entries.Sort(SortEntriesForInjection);
			_kup.Entries.Reverse();

			// Bound Setup
			foreach (Bound bound in _kup.StringBounds)
				bound.NextAvailableOffset = bound.StartLong;

			// Begin Injection
			FileStream fs = new FileStream(codeFile.FullName, FileMode.Open, FileAccess.Write, FileShare.Read);
			BinaryWriterX bw = new BinaryWriterX(fs, ByteOrder.LittleEndian);

			_workerInjector.ReportProgress(0, "STATUS|Injecting strings...");

			bool outOfSpace = false;
			int count = 0;
			foreach (Entry entry in _kup.Entries)
			{
				count++;

				if (entry.Relocatable)
				{
					// Optimization pass
					bool optimized = false;
					foreach (Entry injectedEntry in injectedEntries)
					{
						if (injectedEntry.GetEditedString().EndsWith(entry.GetEditedString()))
						{
							// Update the pointer
							foreach (Pointer pointer in injectedEntry.Pointers)
							{
								bw.BaseStream.Seek(pointer.AddressLong, SeekOrigin.Begin);
								bw.Write((uint)(injectedEntry.OffsetLong + (injectedEntry.EditedText.Length - entry.EditedText.Length) + _kup.RamOffsetUInt));
								optimized = true;
							}
							break;
						}
					}

					if (!optimized)
					{
						// Select bound
						Bound bound = null;
						foreach (Bound stringBound in _kup.StringBounds)
						{
							if (!stringBound.Full && stringBound.Injectable && entry.EditedText.Length < stringBound.SpaceRemaining)
							{
								bound = stringBound;
								break;
							}
						}

						if (bound != null)
						{
							// Update the pointer
							foreach (Pointer pointer in entry.Pointers)
							{
								bw.BaseStream.Seek(pointer.AddressLong, SeekOrigin.Begin);
								bw.Write((uint)(bound.NextAvailableOffset + _kup.RamOffsetUInt));
							}

							// Write the string
							bw.BaseStream.Seek(bound.NextAvailableOffset, SeekOrigin.Begin);
							bw.Write(entry.EditedText);
							if (entry.Encoding.IsSingleByte)
								bw.Write(new byte[] { 0x0 });
							else
								bw.Write(new byte[] { 0x0, 0x0 });
							bound.NextAvailableOffset = bw.BaseStream.Position;

							_workerInjector.ReportProgress(0, "TEXT|" + entry.Encoding.GetString(entry.EditedText).Replace("\0", "<null>") + "\r\n\r\n");

							injectedEntries.Add(entry);
						}
						else
						{
							// Ran out of injection space
							outOfSpace = true;
							break;
						}
					}
				}
				else
				{
					// In place string update
					bw.BaseStream.Seek(entry.OffsetLong, SeekOrigin.Begin);
					bw.Write(entry.EditedText, 0, Math.Min(entry.EditedText.Length, entry.MaxLength));
					if (entry.Encoding.IsSingleByte)
						bw.Write(new byte[] { 0x0 });
					else
						bw.Write(new byte[] { 0x0, 0x0 });

					_workerInjector.ReportProgress(0, "TEXT|" + entry.Encoding.GetString(entry.EditedText).Replace("\0", "<null>") + "\r\n\r\n");
				}

				_workerInjector.ReportProgress((int)(((double)count) / ((double)_kup.Entries.Count) * 100), "BOTTOM");
			}

			if (outOfSpace)
			{
				_workerInjector.ReportProgress(0, "STATUS|The injector has run out of space to inject strings.");
				_workerInjector.ReportProgress(0, "STATUS|Injected " + injectedEntries.Count + " strings...");
				_workerInjector.ReportProgress(0, "STATUS|" + (_kup.Entries.Count - count) + " strings were not injected.");
			}
			else
			{
				_workerInjector.ReportProgress(0, "STATUS|Injected " + injectedEntries.Count + " strings...");
			}

			bw.Close();
		}

		private void workerInjector_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			string[] items = e.UserState.ToString().Split('|');

			switch (items[0])
			{
				case "BOTTOM":
					prgBottom.Value = Math.Max(Math.Min(e.ProgressPercentage, 100), 0);
					break;
				case "STATUS":
					lstStatus.Items.Add(items[1]);
					if(lstStatus.Items.Count > 0)
						lstStatus.SelectedIndex = lstStatus.Items.Count - 1;
					break;
			}
		}

		private void workerInjector_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			prgBottom.Value = 100;
			tlsMain.Enabled = true;
			tlsPointerTables.Enabled = true;
			tlsStringBounds.Enabled = true;
			UpdateForm();
		}

		// Toolbar
		private void tsbPointerTableAdd_Click(object sender, EventArgs e)
		{
			Bound bound = new Bound();
			frmBound frm = new frmBound(bound);

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

			if (frm.ShowDialog() == DialogResult.OK)
			{
				_hasChanges = true;
				LoadForm();
			}
		}

		private void tsbPointerTableDelete_Click(object sender, EventArgs e)
		{
			Bound bound = (Bound)lstPointerTables.SelectedItem;
			if (MessageBox.Show("Are you sure you want to remove " + bound.Start + " to " + bound.End + "?", "Delete Bound?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				_kup.PointerTables.Remove(bound);
				_hasChanges = true;
				LoadForm();
			}
		}

		private void tsbStringBoundAdd_Click(object sender, EventArgs e)
		{
			Bound bound = new Bound();
			bound.Dumpable = true;
			frmBound frm = new frmBound(bound);

			if (frm.ShowDialog() == DialogResult.OK)
			{
				_kup.StringBounds.Add(bound);
				_hasChanges = true;
				LoadForm();
			}
		}

		private void tsbStringBoundProperties_Click(object sender, EventArgs e)
		{
			frmBound frm = new frmBound((Bound)lstStringBounds.SelectedItem);

			if (frm.ShowDialog() == DialogResult.OK)
			{
				_hasChanges = true;
				LoadForm();
			}
		}

		private void tsbStringBoundDelete_Click(object sender, EventArgs e)
		{
			Bound bound = (Bound)lstStringBounds.SelectedItem;
			if (MessageBox.Show("Are you sure you want to remove " + bound.Start + " to " + bound.End + "?", "Delete Bound?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				_kup.StringBounds.Remove(bound);
				_hasChanges = true;
				LoadForm();
			}
		}

		// Events
		private void chkCleanDump_CheckedChanged(object sender, EventArgs e)
		{
			Settings.Default.CleanDump = chkCleanDump.Checked;
			Settings.Default.Save();
		}

		// List
		private void lstPointerTables_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateForm();
		}

		private void lstStringBounds_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateForm();
		}

		private void lstPointerTables_DoubleClick(object sender, EventArgs e)
		{
			tsbPointerTableProperties_Click(sender, e);
		}

		private void lstStringBounds_DoubleClick(object sender, EventArgs e)
		{
			tsbStringBoundProperties_Click(sender, e);
		}

		private void cmsStatus_Opening(object sender, CancelEventArgs e)
		{
			if (lstStatus.SelectedItem == null || lstStatus.SelectedItem.GetType() != typeof(ListItem))
				e.Cancel = true;
		}

		private void copyOffsetToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (lstStatus.SelectedItem != null && lstStatus.SelectedItem.GetType() == typeof(ListItem))
				Clipboard.SetText((string)((ListItem)lstStatus.SelectedItem).Value);
		}

		private void lstStatus_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control & e.KeyCode == Keys.C)
				if (lstStatus.SelectedItem != null && lstStatus.SelectedItem.GetType() == typeof(ListItem))
					Clipboard.SetText((string)((ListItem)lstStatus.SelectedItem).Value);
		}

		private void lstStatus_MouseDown(object sender, MouseEventArgs e)
		{
			lstStatus.SelectedIndex = lstStatus.IndexFromPoint(e.X, e.Y);
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