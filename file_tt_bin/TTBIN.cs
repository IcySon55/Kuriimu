using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using KuriimuContract;

namespace file_ttbin
{
	public sealed class TTBIN
	{
		public List<Label> Labels = new List<Label>();
		public Encoding FileEncoding = Encoding.Unicode;

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		struct Header
		{
			public uint entryCount;
			public uint dataOffset;
			public uint dataLength;
		}

		public TTBIN(string filename)
		{
			using (BinaryReaderX br = new BinaryReaderX(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
			{
				//Header
				var header = br.ReadStruct<Header>();
				throw new Exception(header.entryCount.ToString());
			}
		}
	}
}