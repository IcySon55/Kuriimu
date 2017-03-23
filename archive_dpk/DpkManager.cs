using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KuriimuContract;

namespace archive_dpk
{
	public class DpkManager : IArchiveManager
	{
		private FileInfo _fileInfo = null;
		//private DPK4 _dpk4 = null;

		#region Properties

		// Information
		public string Name => Properties.Settings.Default.PluginName;
		public string Description => "Data Package v4";
		public string Extension => "*.dpk";
		public string About => "This is the DPK4 archive manager for Karameru.";

		// Feature Support
		public bool ArchiveHasExtendedProperties => false;
		public bool CanAddFiles => false;
		public bool CanRenameFiles => false;
		public bool CanDeleteFiles => false;
		public bool CanSave => false;

		public FileInfo FileInfo
		{
			get
			{
				return _fileInfo;
			}
			set
			{
				_fileInfo = value;
			}
		}

		#endregion

		public bool Identify(string filename)
		{
			using (var br = new BinaryReaderX(File.OpenRead(filename)))
			{
				if (br.BaseStream.Length < 4) return false;
				string magic = br.ReadString(4);
				return magic == "DPK4";
			}
		}

		public LoadResult Load(string filename)
		{
			LoadResult result = LoadResult.Success;

			_fileInfo = new FileInfo(filename);

			//if (_fileInfo.Exists)
			//	_dpk4 = new DPK4(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read));
			//else
			//	result = LoadResult.FileNotFound;

			return result;
		}

		public SaveResult Save(string filename = "")
		{
			SaveResult result = SaveResult.Success;

			if (filename.Trim() != string.Empty)
				_fileInfo = new FileInfo(filename);

			try
			{
				//_dpk4.Save(new FileStream(_fileInfo.FullName, FileMode.Create, FileAccess.Write));
			}
			catch (Exception)
			{
				result = SaveResult.Failure;
			}

			return result;
		}

		public CompressionType Compression { get; set; }

		// Files
		public IEnumerable<ArchiveFileInfo> Files
		{
			get
			{
				var files = new List<ArchiveFileInfo>();

				var file = new ArchiveFileInfo();
				file.Filename = "somefile1.ext";
				files.Add(file);

				file = new ArchiveFileInfo();
				file.Filename = "somefile2.ext";
				files.Add(file);

				file = new ArchiveFileInfo();
				file.Filename = "dir1/subfile1.ext";
				files.Add(file);

				file = new ArchiveFileInfo();
				file.Filename = "dir1/subfile2.ext";
				files.Add(file);

				file = new ArchiveFileInfo();
				file.Filename = "dir1/subdir2/filez.ext";
				files.Add(file);

				file = new ArchiveFileInfo();
				file.Filename = "dir2/zilla.ext";
				files.Add(file);

				file = new ArchiveFileInfo();
				file.Filename = "dir2/somefile.ext";
				files.Add(file);

				return files;
			}
		}

		public bool AddFile(ArchiveFileInfo afi)
		{
			return false;
		}

		public byte[] GetFile(ArchiveFileInfo afi)
		{
			return new byte[] { 64, 64, 64, 64 };
		}

		public bool DeleteFile(ArchiveFileInfo afi)
		{
			return false;
		}

		// Features
		public bool ShowProperties(Icon icon)
		{
			return false;
		}
	}
}