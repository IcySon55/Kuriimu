using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using KuriimuContract;
using Cetera;

namespace image_bclim
{
	class BCLIM
	{
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct Header
		{
			public Magic magic; // CLIM
			public ByteOrder byteOrder;
			public int size;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
			private byte[] to_be_finished;
			public Magic magic2;
			public int stuff;
			public short width;
			public short height;
			public Format format;
			public ImageCommon.Orientation orientation;
			byte b1;
			byte b2;
			int imgSize;
		}

		public enum Format : byte
		{
			L8, A8, LA44, LA88, HL88,
			RGB565, RGB888, RGBA5551,
			RGBA4444, RGBA8888,
			ETC1, ETC1A4, L4, A4
		}

		public static Bitmap Load(Stream input)
		{
			using (var br = new BinaryReaderX(input))
			{
				var texture = br.ReadBytes((int)br.BaseStream.Length - 40);
				var header = br.ReadStruct<Header>();
				var settings = new ImageCommon.Settings
				{
					Width = header.width,
					Height = header.height,
					Orientation = header.orientation
				};
				settings.SetFormat(header.format);
				return ImageCommon.Load(texture, settings);
			}
		}
	}
}