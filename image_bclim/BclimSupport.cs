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
		public short header_size;
		public int version;
		public int file_size;
		public int section_count;
		public Magic magic2; // imag
		public int imag_section_size;
		public short width;
		public short height;
		public Format format;
		public ImageCommon.Orientation orientation;
		public short padding;
		public int image_size;
	}

	public enum Format : byte
	{
		L8, A8, LA44, LA88, HL88,
		RGB565, RGB888, RGBA5551,
		RGBA4444, RGBA8888,
		ETC1, ETC1A4, L4, A4
	}
}