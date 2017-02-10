using System.Runtime.InteropServices;
using Cetera;
using KuriimuContract;

namespace image_bclim
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
}