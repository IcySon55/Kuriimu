using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontract.Interface;
using Kontract;
using System.IO;
using Kontract.IO;

namespace archive_nintendo.XCI
{
    public class XCIFileInfo : ArchiveFileInfo
    {

    }

    public enum CartridgeSize : byte
    {
        _2GB = 0xF8,
        _4GB = 0xF0,
        _8GB = 0xE0,
        _16GB = 0xE1,
        _32GB = 0xE2
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Header
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x100)]
        public byte[] rsa;  //RSA-2048 PKCS#1 over 0x100-0x200
        public Magic magic;
        public int secureOffsetUnits;
        public int const1;
        public byte unk1;
        public CartridgeSize cartSize;
        public byte unk2;
        public byte unk3;
        public long unk4;
        public long cartSizeUnits;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] reversedIV;
        public long hfs0Offset;
        public long hfs0HeaderSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public byte[] sha256hfs0Header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public byte[] sha256cryptoHeader;
        public int const2;
        public int const3;
        public int const4;
        public int secureOffsetUnits2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x70)]
        public byte[] encryptedHeader;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class HFS0Header
    {
        public Magic magic;
        public int fileCount;
        public int stringTableSize;
        public int reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class HFS0Entry
    {
        public long offset;
        public long size;
        public int nameOffset;
        public int hashedSize;
        public long reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public byte[] hash;
    }

    public class HFS0NamedEntry
    {
        public string name;
        public HFS0Entry entry;
    }
}
