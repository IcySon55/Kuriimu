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
		public List<UMSBTFileInfo> Files = new List<UMSBTFileInfo>();
		private FileStream _fileStream = null;

		public UMSBT(FileStream fs)
		{
			_fileStream = fs;
			var br = new KuriimuContract.BinaryReaderX(fs);

			uint index = 0;
			while (br.BaseStream.Position < br.BaseStream.Length)
			{
				var info = new UMSBTFileInfo();
				info.Entry = br.ReadStruct<UMSBTFileEntry>();
				info.FileName = index.ToString("00000000") + ".msbt";
				info.FileData = new SubStream(fs, info.Entry.Offset, info.Entry.Size);
				info.State = ArchiveFileState.Archived;

				if (info.Entry.Offset == 0 && info.Entry.Size == 0)
					break;
				else
					Files.Add(info);

				index++;
			}
		}

		public bool Save(FileStream fs)
		{
			bool result = true;

			try
			{
				using (var bw = new KuriimuContract.BinaryWriterX(fs))
				{
					uint padding = 24;
					uint headerLength = ((uint)Files.Count) * (sizeof(uint) * 2) + padding;
					uint runningTotal = 0;

					foreach (var info in Files)
					{
						info.Entry.Offset = headerLength + runningTotal;
						info.Entry.Size = (uint)info.FileData.Length;

						runningTotal += (uint)info.FileData.Length;
						bw.WriteStruct(info.Entry);
					}

					for (int i = 0; i < padding; i++)
						bw.Write((byte)0x0);

					foreach (var info in Files)
					{
						info.FileData.CopyTo(bw.BaseStream);
					}
				}
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		public void Close()
		{
			_fileStream?.Dispose();
			_fileStream = null;
		}
	}
}