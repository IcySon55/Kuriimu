﻿using System.Runtime.InteropServices;
using Kontract.Interface;

namespace archive_nintendo.UMSBT
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class UmsbtFileEntry
    {
        public uint Offset = 0;
        public uint Size = 0;
    }

    public class UmsbtFileInfo : ArchiveFileInfo
    {
        public UmsbtFileEntry Entry = null;
    }
}
