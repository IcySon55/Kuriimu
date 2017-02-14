using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cetera.Image;
using KuriimuContract;

namespace image_jtex
{
	class JtexAdapter : IImageAdapter
	{
		private FileInfo _fileInfo = null;
		private JTEX _jtex = null;

		#region Properties

		// Information
		public string Name => Properties.Settings.Default.PluginName;

		public string Description => "J Texture";

		public string Extension => "*.jtex";

		public string About => "This is the JTEX file adapter for Kukkii.";

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
				if (br.BaseStream.Length < 4) return false;
				return br.ReadString(4) == "jIMG";
			}
		}

		public LoadResult Load(string filename)
		{
			LoadResult result = LoadResult.Success;

			_fileInfo = new FileInfo(filename);

			if (_fileInfo.Exists)
				_jtex = new JTEX(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read));
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
				_jtex.Save(new FileStream(_fileInfo.FullName, FileMode.Create, FileAccess.Write));
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
				return _jtex.Image;
			}
			set
			{
				_jtex.Image = value;
			}
		}
	}
}