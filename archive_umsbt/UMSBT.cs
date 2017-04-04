using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cetera.IO;
using KuriimuContract;

namespace archive_umsbt
{
	public class InvalidUMSBTException : Exception
	{
		public InvalidUMSBTException(string message) : base(message) { }
	}

	public class UMSBT
	{
		public List<ArchiveFileInfo> Files = new List<ArchiveFileInfo>();
		private FileStream _fileStream = null;

		public UMSBT(FileStream fs)
		{
			_fileStream = fs;
			var br = new KuriimuContract.BinaryReaderX(fs);

			// Entries
			uint index = 0;

			while (br.BaseStream.Position < br.BaseStream.Length)
			{
				var info = new UMSBTFileInfo();
				info.Entry = br.ReadStruct<UMSBTFileEntry>();
				info.Index = index++;
				info.FileName = info.Index.ToString("00000000") + ".msbt";
				info.FileData = new SubStream(fs, info.Entry.Offset, info.Entry.Size);
				info.State = ArchiveFileState.Archived;

				if (info.Entry.Offset == 0 && info.Entry.Size == 0)
					break;
				else
					Files.Add(info);
			}
		}

		public bool Save(FileStream fs)
		{
			using (var bw = new KuriimuContract.BinaryWriterX(fs))
			{
				uint offsetsLength = ((uint)Files.Count + 1) * (sizeof(uint) * 2);
				uint runningTotal = 0;

				foreach (var info in Files)
				{
					info.FileData.Seek(0, SeekOrigin.Begin);
					uint offset = offsetsLength + runningTotal;
					uint size = (uint)info.FileData.Length;
					runningTotal += (uint)info.FileData.Length;

					bw.Write(offset);
					bw.Write(size);
				}

				bw.Write(0x00000000);
				bw.Write(0x00000000);

				foreach (var info in Files)
				{
					info.FileData.CopyTo(bw.BaseStream);
				}
			}

			return true;
		}
	}
}