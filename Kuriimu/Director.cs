using Kuriimu.Properties;
using KuriimuContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Kuriimu
{
	public partial class frmDirector : Form
	{
		private uint ram = 0x100000; // 3DS

		public frmDirector()
		{
			InitializeComponent();
		}

		private void Director_Load(object sender, EventArgs e)
		{
			Text = Settings.Default.ApplicationName + " Director " + Settings.Default.ApplicationVersion;
			Icon = Properties.Resources.kuriimu;

			txtFile.Text = Settings.Default.DirectorFile;
			txtOffset.Text = Settings.Default.DirectorOffset;
			txtLeneance.Text = Settings.Default.DirectorLeneance;

			if (txtOffset.Text.Trim().Length == 0)
				txtOffset.Text = "00";
			if (txtLeneance.Text.Trim().Length == 0)
				txtLeneance.Text = "0";
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog ofd = new OpenFileDialog())
			{
				ofd.InitialDirectory = Application.StartupPath;
				ofd.Multiselect = false;

				if (ofd.ShowDialog() == DialogResult.OK)
				{
					txtFile.Text = ofd.FileName;
				}
			}
		}

		private void btnLookup_Click(object sender, EventArgs e)
		{
			Encoding enc = Encoding.Unicode;
			FileInfo file = new FileInfo(txtFile.Text.Trim());

			if (file.Exists)
			{
				using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					BinaryReaderX br = new BinaryReaderX(fs, ByteOrder.LittleEndian);

					txtResults.Text = string.Empty;
					uint offset = Convert.ToUInt32(txtOffset.Text.Trim(), 16);
					uint leneance = Convert.ToUInt32(txtLeneance.Text.Trim());

					uint value = 0;
					List<uint> pointers = new List<uint>();

					while (br.BaseStream.Position < br.BaseStream.Length - 4)
					{
						bool found = false;
						uint pointer = (uint)br.BaseStream.Position;
						value = br.ReadUInt32();

						if (value == (offset + ram))
							found = true;
						else if (value >= (offset + ram - leneance) && value < offset + ram)
							found = true;
						else if (value <= (offset + ram + leneance) && value > offset + ram)
							found = true;

						if (found)
							pointers.Add(pointer);
					}

					if (pointers.Count > 0)
					{
						List<byte> result = new List<byte>();
						br.BaseStream.Seek(offset, SeekOrigin.Begin);
						while (br.BaseStream.Position < br.BaseStream.Length)
						{
							byte[] unichar = br.ReadBytes(2);

							if (unichar[0] != 0x0 || unichar[1] != 0x0)
								result.AddRange(unichar);
							else
								break;
						}

						string strResult = enc.GetString(result.ToArray());

						txtResults.AppendText("<entry encoding=\"Unicode\" name=\"\" offset=\"" + offset.ToString("X2") + "\" relocatable=\"true\" max_length=\"0\">");
						txtResults.AppendText("\r\n");

						foreach (uint p in pointers)
						{
							txtResults.AppendText("\t<pointer address=\"" + p.ToString("X2") + "\" />");
							txtResults.AppendText("\r\n");
						}

						txtResults.AppendText("\t<original>" + strResult + "</original>");
						txtResults.AppendText("\r\n");
						txtResults.AppendText("\t<edited>" + strResult + "</edited>");
						txtResults.AppendText("\r\n");
						txtResults.AppendText("</entry>");
					}
					else
					{
						txtResults.Text = "No pointers found...";
					}

					br.Close();
				}
			}
		}

		private void txtFile_TextChanged(object sender, EventArgs e)
		{
			Settings.Default.DirectorFile = txtFile.Text;
			Settings.Default.Save();
		}

		private void txtOffset_TextChanged(object sender, EventArgs e)
		{
			Settings.Default.DirectorOffset = txtOffset.Text;
			Settings.Default.Save();
		}

		private void txtLeneance_TextChanged(object sender, EventArgs e)
		{
			Settings.Default.DirectorLeneance = txtLeneance.Text;
			Settings.Default.Save();
		}
	}
}