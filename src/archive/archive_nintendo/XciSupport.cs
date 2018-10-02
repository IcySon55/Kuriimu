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
        public int backupOffsetUnits;   //Always 0xFFFFFFFF
        public byte kekIndex;   //high nibble: title key index / low nibble: kek index
        public CartridgeSize cartSize;
        public byte cartHeaderVerison;
        public byte cartFlags;      //bit0: Autoboot / bit1: HistoryErase
        public long packageId;      //used for authentication
        public long cartSizeUnits;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] reversedIV;
        public long hfs0Offset;
        public long hfs0HeaderSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public byte[] sha256hfs0Header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        public byte[] sha256cryptoHeader;
        public int secureModeFlag;      //1 means secure mode is used
        public int titleKeyFlag;        //always 2
        public int keyFlag;             //always 0
        public int secureOffsetUnits2;
        public GameCardInfo gameCardInfo;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class GameCardInfo
    {
        public long firmwareVersion;        //0x1: old games / 0x2: new games with logo partition
        public int accessControlFlags;
        public int readWaitTime;
        public int readWaitTime2;
        public int writeWaitTime;
        public int writeWaitTime2;
        public int firmwareMode;
        public int cupVersion;
        public int zero1;
        public long updatePartitionHash;
        public long cupId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x38)]
        public byte[] zero2;
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
