using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Linq;

namespace archive_xc2
{
    public class XC2
    {
        public List<ArchiveFileInfo> Files = new List<ArchiveFileInfo>();
        private Stream _index = null;
        private Stream _files = null;

        private byte[] _xorKey = new byte[] { 0x33, 0xb5, 0xe2, 0x5d };

        private Header _header;
        private List<StringEntry> _nameEntries;
        private List<NodeEntry> _nodeEntries;
        private List<FileEntry> _fileEntries;

        public XC2(Stream indexFile, Stream fileData)
        {
            _index = indexFile;
            _files = fileData;

            using (var fileBr = new BinaryReaderX(_files, true))
            using (var indexBr = new BinaryReaderX(_index, true))
            {
                _header = indexBr.ReadStruct<Header>();

                ReadStrings();
                ReadNodes();
                indexBr.BaseStream.Position = _header.fileEntryTableOffset;
                _fileEntries = indexBr.ReadMultiple<FileEntry>(_header.fileEntryCount);

                //get filenames
                for (int i = 0; i < _fileEntries.Count; i++)
                {
                    var nameEntry = _nameEntries[i];
                    foreach (var usedNode in _nodeEntries.Where(x => x.usedNode))
                    {
                        if (usedNode.id1 <= nameEntry.offset && usedNode.id1 > (nameEntry.offset - nameEntry.name.Length))
                        {
                            var fileNameOffset = usedNode.id1;
                        }
                    }
                }
            }
        }

        private void LoopNodes(NodeEntry usedNode)
        {

        }

        private void ReadStrings()
        {
            using (var nameBr = new BinaryReaderX(new XorStream(new SubStream(_index, _header.filenameTableOffset, _header.filenameTableSize), _xorKey), true))
            {
                var nameActualSize = nameBr.ReadInt32();
                if (_header.filenameTableSizeUnpadded != nameActualSize)
                    throw new InvalidDataException("filenameTableSize doesn't match header value.");

                _nameEntries = new List<StringEntry>();
                while (nameBr.BaseStream.Position < nameActualSize)
                    _nameEntries.Add(new StringEntry { offset = (int)-nameBr.BaseStream.Position, name = nameBr.ReadCStringA(), id = nameBr.ReadInt32() });
            }
        }

        private void ReadNodes()
        {
            using (var nodeBr = new BinaryReaderX(new XorStream(new SubStream(_index, _header.trieTableOffset, _header.trieTableSize), _xorKey), true))
            {
                _nodeEntries = new List<NodeEntry>();
                while (nodeBr.BaseStream.Position < nodeBr.BaseStream.Length)
                {
                    var id1 = nodeBr.ReadInt32();
                    var id2 = nodeBr.ReadInt32();
                    _nodeEntries.Add(new NodeEntry { id1 = id1, id2 = id2, usedNode = id2 > -1 });
                }
            }
        }

        public void Save(Stream output)
        {
            using (var bw = new BinaryWriterX(output))
            {
            }
        }

        public void Close()
        {
            _index?.Dispose();
            _files?.Dispose();
            foreach (var afi in Files)
                if (afi.State != ArchiveFileState.Archived)
                    afi.FileData?.Dispose();
            _index = null;
            _files = null;
        }
    }
}
