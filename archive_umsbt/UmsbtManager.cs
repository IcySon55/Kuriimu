using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cetera.IO;
using KuriimuContract;

namespace archive_umsbt
{
	public class UmsbtManager : IArchiveManager
	{
		private FileInfo _fileInfo = null;
		private UMSBT _umsbt = null;

		#region Properties

		// Information
		public string Name => Properties.Settings.Default.PluginName;
		public string Description => "UMSBT Archive";
		public string Extension => "*.umsbt";
		public string About => "This is the UMSBT archive manager for Karameru.";

		// Feature Support
		public bool ArchiveHasExtendedProperties => false;
		public bool CanAddFiles => false;
		public bool CanRenameFiles => false;
		public bool CanReplaceFiles => true;
		public bool CanDeleteFiles => false;
		public bool CanSave => true;

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
			// TODO: Make this way more robust
			return filename.EndsWith(".umsbt");
		}

		public LoadResult Load(string filename)
		{
			LoadResult result = LoadResult.Success;

			_fileInfo = new FileInfo(filename);

			if (_fileInfo.Exists)
				_umsbt = new UMSBT(_fileInfo.OpenRead());
			else
				result = LoadResult.FileNotFound;

			return result;
		}

		public SaveResult Save(string filename = "")
		{
			SaveResult result = SaveResult.Success;

			if (filename.Trim() != string.Empty)
				_fileInfo = new FileInfo(filename);

			try
			{
				// Save As...
				if (filename.Trim() != string.Empty)
					_umsbt.Save(File.Create(filename));
				else
				{
					// Create the temp file
					_umsbt.Save(File.Create(filename + ".tmp"));
					// Delete the original
					File.Delete(filename);
					// Rename the temporary file
					File.Move(filename + ".tmp", filename);
				}

				// Reload the new file to make sure everything is in order
				Load(filename);
			}
			catch (Exception)
			{
				result = SaveResult.Failure;
			}

			return result;
		}

		// Files
		public IEnumerable<ArchiveFileInfo> Files
		{
			get
			{
				return _umsbt.Files;
			}
		}

		public bool AddFile(ArchiveFileInfo afi)
		{
			return false;
		}

		public bool RenameFile(ArchiveFileInfo afi)
		{
			return false;
		}

		public bool ReplaceFile(ArchiveFileInfo afi)
		{
			return false;
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