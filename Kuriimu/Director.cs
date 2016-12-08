using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using KuriimuContract;

namespace Kuriimu
{
	public partial class Director : Form
	{
		public string[] Arguments { get; set; }
		private uint ram = 0x100000; // 3DS

		public Director()
		{
			InitializeComponent();
		}

		private void Director_Load(object sender, EventArgs e)
		{
			txtOffset.Text = "0";
			txtLeneance.Text = "0";
			this.Icon = Properties.Resources.kuriimu;
		}

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.InitialDirectory = Application.StartupPath;
			ofd.Multiselect = false;

			if (ofd.ShowDialog() == DialogResult.OK)
			{
				txtFile.Text = ofd.FileName;
			}
		}

		private void btnLookup_Click(object sender, EventArgs e)
		{
			Encoding enc = Encoding.Unicode;
			FileInfo file = new FileInfo(txtFile.Text.Trim());

			if (file.Exists)
			{
				FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
				BinaryReaderX br = new BinaryReaderX(fs, ByteOrder.LittleEndian);

				txtResults.Text = string.Empty;
				uint offset = Convert.ToUInt32(txtOffset.Text.Trim(), 16);
				uint leneance = Convert.ToUInt32(txtLeneance.Text.Trim());

				Console.WriteLine("Working...");

				bool found = false;
				uint value = 0;
				long next = 0x0;

				while (br.BaseStream.Position < br.BaseStream.Length - 4)
				{
					found = false;
					value = br.ReadUInt32();
					next = br.BaseStream.Position;

					if (value == (offset + ram))
						found = true;
					else if (value >= (offset + ram - leneance) && value < offset + ram)
						found = true;
					else if (value <= (offset + ram + leneance) && value > offset + ram)
						found = true;

					if (found)
					{
						Console.WriteLine("Found!");
						txtResults.AppendText("\r\n");
						txtResults.AppendText("\t\t<entry offset=\"" + (value - ram).ToString("X2") + "\" pointer=\"" + (br.BaseStream.Position - 4).ToString("X2") + "\">");
						txtResults.AppendText("\r\n");

						List<byte> result = new List<byte>();
						br.BaseStream.Seek(offset, SeekOrigin.Begin);
						while (br.BaseStream.Position < br.BaseStream.Length)
						{
							byte[] unichar = br.ReadBytes(2);

							if (unichar[0] != 0x0 || unichar[1] != 0x0)
								result.AddRange(unichar);
							else
							{
								if (!found)
								{
									txtResults.AppendText("\r\n");
									txtResults.AppendText("\t\t<entry offset=\"" + offset.ToString("X2") + "\" max-length=\"" + 21 + "\">");
									txtResults.AppendText("\r\n");
								}
								txtResults.AppendText("\t\t\t<text>" + enc.GetString(result.ToArray()) + "</text>");
								txtResults.AppendText("\r\n");
								txtResults.AppendText("\t\t\t<translation>" + enc.GetString(result.ToArray()) + "</translation>");
								txtResults.AppendText("\r\n");
								txtResults.AppendText("\t\t</entry>");
								break;
							}
						}

						br.BaseStream.Seek(next, SeekOrigin.Begin);
					}
				}

				br.Close();
				Console.WriteLine("Done~");
			}
		}
	}
}