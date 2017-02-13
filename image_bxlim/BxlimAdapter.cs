using System;
using System.Drawing;
using System.IO;
using Cetera.Image;
using KuriimuContract;

namespace image_bxlim
{
	public class BxlimAdapter : IImageAdapter
	{
		private FileInfo _fileInfo = null;
		private BXLIM _bxlim = null;

		#region Properties

		// Information
		public string Name => Properties.Settings.Default.PluginName;

		public string Description => "Binary Layout Image";

		public string Extension => "*.bclim;*bflim";

		public string About => "This is the BCLIM and BFLIM file adapter for Kukkii.";

		// Feature Support
		public bool FileHasExtendedProperties => false;

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
			using (var br = new BinaryReaderX(File.OpenRead(filename)))
			{
				if (br.BaseStream.Length < 40) return false;
				br.BaseStream.Seek((int)br.BaseStream.Length - 40, SeekOrigin.Begin);
				string magic = br.ReadString(4);
				return magic == "CLIM" || magic == "FLIM";
			}
		}

		public LoadResult Load(string filename)
		{
			LoadResult result = LoadResult.Success;

			_fileInfo = new FileInfo(filename);

			if (_fileInfo.Exists)
				_bxlim = new BXLIM(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read));
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
				_bxlim.Save(new FileStream(_fileInfo.FullName, FileMode.Create, FileAccess.Write));
			}
			catch (Exception)
			{
				result = SaveResult.Failure;
			}

			return result;
		}

		// Bitmaps
		public Bitmap Bitmap
		{
			get
			{
				return _bxlim.Image;
			}
			set
			{
				_bxlim.Image = value;
			}
		}
	}
}