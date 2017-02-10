using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using KuriimuContract;

namespace image_xi
{
	public sealed class XiAdapter : IImageAdapter
	{
		private FileInfo _fileInfo = null;
		private Bitmap _xi = null;

		public string Name => "XI";
		public string Description => "Level 5 Compressed Image";
		public string Extension => "*.xi";
		public string About => "This is the XI file adapter for Kukkii.";

		// Feature Support
		public bool FileHasExtendedProperties => false;
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

		public bool Identify(string filename)
		{
			using (var br = new BinaryReaderX(File.OpenRead(filename)))
			{
				if (br.BaseStream.Length < 4) return false;
				return br.ReadString(4) == "IMGC";
			}
		}

		public LoadResult Load(string filename)
		{
			LoadResult result = LoadResult.Success;

			_fileInfo = new FileInfo(filename);

			if (_fileInfo.Exists)
				_xi = XI.Load(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read));
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
				//_msbt.Save(_fileInfo.FullName);
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
				return _xi;
			}
			set
			{
				_xi = value;
			}
		}
	}
}