using System.Drawing;
using System.IO;
using Cetera;
using KuriimuContract;

namespace image_bclim
{
	class BCLIM
	{
		public Header Header { get; }

		public Bitmap Image { get; set; }

		public BCLIM(Stream input)
		{
			using (var br = new BinaryReaderX(input))
			{
				var texture = br.ReadBytes((int)br.BaseStream.Length - 40);
				Header = br.ReadStruct<Header>();
				var settings = new ImageCommon.Settings
				{
					Width = Header.width,
					Height = Header.height,
					Orientation = Header.orientation,
					Format = ImageCommon.Settings.ConvertFormat(Header.format)
				};
				Image = ImageCommon.Load(texture, settings);
			}
		}

		public void Save(Stream output)
		{
			using (var bw = new BinaryWriterX(output))
			{
				var settings = new ImageCommon.Settings()
				{
					Width = Header.width,
					Height = Header.height,
					Orientation = Header.orientation,
					Format = ImageCommon.Settings.ConvertFormat(Header.format)
				};
				var texture = ImageCommon.Save(Image, settings);
				bw.Write(texture);
				bw.WriteStruct(Header);
			}
		}
	}
}