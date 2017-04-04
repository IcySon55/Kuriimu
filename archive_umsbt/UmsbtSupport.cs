using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using KuriimuContract;

namespace archive_umsbt
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class UMSBTFileEntry
	{
		public uint Offset = 0;
		public uint Size = 0;
	}

	public class UMSBTFileInfo : ArchiveFileInfo, IComparable<UMSBTFileInfo>
	{
		public UMSBTFileEntry Entry = null;
		public uint Index = 0;

		public int CompareTo(UMSBTFileInfo rhs) => Index.CompareTo(rhs.Index);
	}
}