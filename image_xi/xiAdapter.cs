using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KuriimuContract;
using image_xi;

namespace image_xi
{
	public sealed class xiAdapter:IImageAdapter
	{
		private FileInfo _fileInfo = null;
		private Bitmap _xi = null;
		private List<Bitmap> _bitmaps = null;

		public string Name => "image_xi";
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

		//Identify if plugin can handle input file
		public bool Identify(string filename)
		{
			bool result = true;

			try
			{
				XI.Load(new FileStream(filename, FileMode.Open, FileAccess.Read));
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		public LoadResult Load(string filename)
		{
			LoadResult result = LoadResult.Success;

			_fileInfo = new FileInfo(filename);
			_bitmaps = null;

			if (_fileInfo.Exists)
			{
				try
				{
					_xi = XI.Load(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read));
				}
				catch (Exception)
				{
					result = LoadResult.Failure;
				}
			}
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
		public IEnumerable<Bitmap> Bitmaps
		{
			get
			{
				if (_bitmaps == null)
				{
					_bitmaps = new List<Bitmap>();
					_bitmaps.Add(_xi);
				}

				return _bitmaps;
			}
		}
	}
}
