using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Kuriimu.Contract;
using Kuriimu.IO;

namespace archive_999
{
    public class A999
    {
        public List<A999FileInfo> Files;

        const uint headXORpad = 0xFABACEDA;

        static uint Hash999(string s) => (uint)(s.Aggregate(0, (n, c) => n * 131 + (c & ~32)) * 16 | s.Sum(c => c) % 16) * 2 / 2;
        static Lazy<Dictionary<uint, string>> dicFoldersInitializer = new Lazy<Dictionary<uint, string>>(() =>
        {
            IEnumerable<string> spl = "/bg/a01|/bg/a11|/bg/a12|/bg/a21|/bg/a31|/bg/a32|/bg/a41|/bg/a42|/bg/b11|/bg/b12|/bg/b21|/bg/b31|/bg/b32|/bg/c21|/bg/c31|/bg/c32|/bg/minigame/calculator|/bg/minigame/description|/bg/minigame/escape|/bg/minigame/file|/bg/minigame/flow|/bg/minigame/item|/bg/minigame/map|/bg/minigame/novel|/bg/minigame/option|/bg/minigame/room|/bg/minigame/save|/bg/minigame/start|/bg/minigame/title|/bg/minigame/topview|/bg/novel|/bg/sprite|/bg/sprite/alpha|/bg/sprite/button/steam|/bg/sprite/channel|/bg/sprite/circle|/bg/sprite/cursor|/bg/sprite/kigou|/bg/topview|/cha|/etc|/item|/item/texture|/movie|/scr|/scr/table|/shader|/sound|/sound/voice|/sound/voice_us|/temp".Split('|');
            spl = spl.Concat(spl.Select(s => "/us" + s));
            spl = spl.Concat(spl.Select(s => s + "/resource"));
            spl = spl.Concat(spl.Select(s => s + "/outline"));
            return spl.ToDictionary(Hash999);
        });
        static Dictionary<uint, string> dicFolders => dicFoldersInitializer.Value;

        public A999(Stream input)
        {
            using (var br = new BinaryReaderX(new XorStream(input, headXORpad), true))
            {
                //Header
                var header = br.ReadStruct<Header>();

                //Directories
                var directoryTop = br.ReadStruct<TableHeader>();
                var directoryHashes = br.ReadMultiple<uint>(directoryTop.entryCount);
                while (br.BaseStream.Position % 16 != 0) br.BaseStream.Position++;
                var directoryEntries = br.ReadMultiple<DirectoryEntry>(directoryTop.entryCount);

                //FileEntries
                var entryTop = br.ReadStruct<TableHeader>();
                var XORs = br.ReadMultiple<uint>(entryTop.entryCount);
                while (br.BaseStream.Position % 16 != 0) br.BaseStream.Position++;

                //Files
                Files = directoryEntries.SelectMany(dirEntry =>
                {
                    if (!dicFolders.TryGetValue(dirEntry.directoryHash, out var path))
                    {
                        path = $"/TBD/0x{dirEntry.directoryHash:X8}";
                    }
                    return br.ReadMultiple<Entry>(dirEntry.fileCount).Select(entry => new A999FileInfo
                    {
                        Entry = entry,
                        FileName = $"{path}/0x{entry.XORpad:X8}.unk",
                        State = ArchiveFileState.Archived,
                        FileData = new XorStream(new SubStream(input, header.dataOffset + entry.fileOffset, entry.fileSize), entry.XORpad)
                    });
                }).ToList();
            }
        }
    }
}
