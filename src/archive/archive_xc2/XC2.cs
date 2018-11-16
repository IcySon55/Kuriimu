using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;
using System.Linq;
using System;

namespace archive_xc2
{
    public class XC2
    {
        public List<ArchiveFileInfo> Files = new List<ArchiveFileInfo>();
        private Stream _index = null;
        private Stream _files = null;

        private readonly byte[] _xorKey = new byte[] { 0x33, 0xb5, 0xe2, 0x5d };

        private Header _header;
        private List<StringEntry> _nameEntries;
        private List<NodeEntry> _nodeEntries;
        private List<FileEntry> _fileEntries;

        public XC2(Stream indexFile, Stream fileData)
        {
            _index = indexFile;
            _files = fileData;

            //using (var fileBr = new BinaryReaderX(_files, true))
            using (var indexBr = new BinaryReaderX(_index, true))
            {
                _header = indexBr.ReadStruct<Header>();

                //DeXor stringtable
                using (var xor = new XorStream(new SubStream(_index, _header.filenameTableOffset, _header.filenameTableSize), _xorKey))
                using (var br = new BinaryReaderX(xor, true))
                    File.WriteAllBytes(@"C:\Users\\Desktop\nametable.dexor", br.ReadAllBytes());

                //DeXor nodetable
                using (var xor = new XorStream(new SubStream(_index, _header.trieTableOffset, _header.trieTableSize), _xorKey))
                using (var br = new BinaryReaderX(xor, true))
                    File.WriteAllBytes(@"C:\Users\\Desktop\nodetable.dexor", br.ReadAllBytes());

                ReadStrings();
                ReadNodes();

                indexBr.BaseStream.Position = _header.fileEntryTableOffset;
                for (int i = 0; i < _header.fileEntryCount; i++)
                {
                    var coffset = indexBr.ReadUInt64();
                    var csize = indexBr.ReadUInt32();
                    var ucsize = indexBr.ReadUInt32();
                    var compressed = indexBr.ReadUInt32();
                    var id = indexBr.ReadUInt32();

                    var Filename01 = _array0[i];
                    var fileOffset = _array3[i];
                    var filenamelen = Filename01.Length;

                    var Fileend = fileOffset;
                    Fileend -= filenamelen;
                    var cstr = "";
                    var filenameoffset = FindFilename(ref cstr, fileOffset, Fileend);

                    var NAME_LENGTH = cstr.Length;
                    filenameoffset -= (int)fileOffset;
                    filenameoffset = -filenameoffset;

                    if (filenameoffset > 0)
                        Filename01 = Filename01.Substring(filenameoffset, Filename01.Length - filenameoffset);

                    cstr += Filename01;

                    ;
                }

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

        private int FindFilename(ref string cstr, long fileoffset, long Fileend)
        {
            for (int g = 0; g < _array4.Count; g++)
            {
                var f = _array4[g];
                var id1 = _array1[f];
                var id2 = _array2[f];

                if (id1 <= fileoffset && id1 > Fileend)
                {
                    var filenameoffset = id1;
                    var currentChar = f;

                    LoopNodes(id2, currentChar, ref cstr);

                    cstr = cstr.Reverse().Aggregate("", (a, b) => a += b);
                    return filenameoffset;
                }
            }

            return 0;
        }

        private void LoopNodes(int id2, int currentChar, ref string cstr)
        {
            var cci = id2;
            var id1 = _array1[id2];
            id2 = _array2[id2];

            currentChar ^= id1;
            var _cstr = currentChar.ToString();
            cstr += _cstr;
            currentChar = cci;

            if (id2 > 0)
                LoopNodes(id2, currentChar, ref cstr);
        }

        private void LoopNodes(NodeEntry usedNode)
        {

        }

        string[] _array0;
        long[] _array3;
        private void ReadStrings()
        {
            using (var nameBr = new BinaryReaderX(new XorStream(new SubStream(_index, _header.filenameTableOffset, _header.filenameTableSize), _xorKey), true))
            {
                var chunkSize = nameBr.ReadInt32();
                if (_header.filenameTableSizeUnpadded != chunkSize)
                    throw new InvalidDataException("filenameTableSize doesn't match header value.");

                _array0 = new string[_header.fileEntryCount];
                _array3 = new long[_header.fileEntryCount];
                for (int i = 0; i < _header.fileEntryCount; i++)
                {
                    var offset = nameBr.BaseStream.Position;
                    var fname = nameBr.ReadCStringA();
                    _array0[i] = fname;
                    offset = -offset;
                    _array3[i] = offset;
                    var id = nameBr.ReadInt32();
                }
            }
        }

        int[] _array1;
        int[] _array2;
        List<int> _array4;
        private void ReadNodes()
        {
            using (var nodeBr = new BinaryReaderX(new XorStream(new SubStream(_index, _header.trieTableOffset, _header.trieTableSize), _xorKey), true))
            {
                _array1 = new int[_header.nodeCount];
                _array2 = new int[_header.nodeCount];
                _array4 = new List<int>();
                for (int i = 0; i < _header.nodeCount; i++)
                {
                    var id1 = nodeBr.ReadInt32();
                    var id2 = nodeBr.ReadInt32();

                    _array1[i] = id1;
                    _array2[i] = id2;

                    if (id2 > -1)
                        _array4.Add(i);
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
