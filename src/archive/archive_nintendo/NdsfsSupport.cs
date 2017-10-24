using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Kontract.Interface;
using Kontract.IO;
using System.Text;

//source code adjusted from pleonex' github:
//https://github.com/pleonex/tinke/blob/master/Tinke

namespace archive_nintendo.NDSFS
{
    public class NDSFileInfo : ArchiveFileInfo
    {
        public sFile entry;
    }

    public class NdsfsSupport
    {
        public static sFAT[] ReadFAT(Stream input, uint fatOffset, uint fatSize)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                sFAT[] fat = new sFAT[fatSize / 0x08];

                br.BaseStream.Position = fatOffset;

                for (int i = 0; i < fat.Length; i++)
                {
                    fat[i].offset = br.ReadUInt32();
                    fat[i].size = br.ReadUInt32() - fat[i].offset;
                }

                return fat;
            }
        }

        public static sFolder ReadFNT(Stream input, uint fntOffset, uint fntSize, sFAT[] fat)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                sFolder root = new sFolder();
                root.files = new List<sFile>();
                List<MainFNT> mains = new List<MainFNT>();

                br.BaseStream.Position = fntOffset;

                br.BaseStream.Position += 6;
                ushort number_directories = br.ReadUInt16();  // Get the total number of directories (mainTables)
                br.BaseStream.Position = fntOffset;

                for (int i = 0; i < number_directories; i++)
                {
                    MainFNT main = new MainFNT();
                    main.offset = br.ReadUInt32();
                    main.idFirstFile = br.ReadUInt16();
                    main.idParentFolder = br.ReadUInt16();

                    if (i != 0)
                    {
                        if (br.BaseStream.Position > fntOffset + mains[0].offset)
                        {                                      //  Error, in some cases the number of directories is wrong
                            number_directories--;              // Found in FF Four Heroes of Light, Tetris Party deluxe
                            i--;
                            continue;
                        }
                    }

                    long currOffset = br.BaseStream.Position;
                    br.BaseStream.Position = fntOffset + main.offset;

                    // SubTable
                    byte id = br.ReadByte();
                    ushort idFile = main.idFirstFile;

                    while (id != 0x0)   // identifies subTable end
                    {
                        if (id < 0x80)  // File
                        {
                            sFile currFile = new sFile();

                            if (!(main.subTable.files is List<sFile>))
                                main.subTable.files = new List<sFile>();

                            int lengthName = id;
                            currFile.name = new String(Encoding.GetEncoding("SJIS").GetChars(br.ReadBytes(lengthName)));
                            currFile.id = idFile; idFile++;

                            // FAT part
                            currFile.offset = fat[currFile.id].offset;
                            currFile.size = fat[currFile.id].size;

                            // Add to root
                            root.files.Add(currFile);

                            main.subTable.files.Add(currFile);
                        }
                        if (id > 0x80)  //Directory
                        {
                            sFolder currFolder = new sFolder();

                            if (!(main.subTable.folders is List<sFolder>))
                                main.subTable.folders = new List<sFolder>();

                            int lengthName = id - 0x80;
                            currFolder.name = new String(Encoding.GetEncoding("SJIS").GetChars(br.ReadBytes(lengthName))) + "/";
                            currFolder.id = br.ReadUInt16();

                            main.subTable.folders.Add(currFolder);
                        }

                        id = br.ReadByte();
                    }

                    mains.Add(main);
                    br.BaseStream.Position = currOffset;
                }

                root = MakeFolderStructure(mains, 0, "root");
                root.id = number_directories;

                return root;
            }
        }

        public static sFolder AddSystemFiles(Stream input, sFAT[] fatTable, int lastFileID, int lastFolderID, RomHeader header)
        {
            sFolder system = new sFolder();
            system.name = "fw_sys/";
            system.id = (ushort)lastFolderID;
            lastFolderID++;
            system.files = new List<sFile>();
            system.files.AddRange(ReadBasicOverlays(input, header.ARM9overlayOffset, header.ARM9overlaySize, true, fatTable));
            system.files.AddRange(ReadBasicOverlays(input, header.ARM7overlayOffset, header.ARM7overlaySize, false, fatTable));

            sFile fnt = new sFile();
            fnt.name = "fileNameTable.bin";
            fnt.offset = header.fileNameTableOffset;
            fnt.size = header.fileNameTableSize;
            fnt.id = (ushort)lastFolderID;
            lastFolderID++;
            system.files.Add(fnt);

            sFile fat = new sFile();
            fat.name = "fileAccessTable.bin";
            fat.offset = header.FAToffset;
            fat.size = header.FATsize;
            fat.id = (ushort)lastFolderID;
            lastFolderID++;
            system.files.Add(fat);

            sFile banner = new sFile();
            banner.name = "banner.bin";
            banner.offset = header.bannerOffset;
            banner.size = 0x840;
            banner.id = (ushort)lastFolderID;
            lastFolderID++;
            system.files.Add(banner);

            sFile arm9 = new sFile();
            arm9.name = "arm9.bin";
            arm9.offset = header.ARM9romOffset;
            arm9.size = header.ARM9size;
            arm9.id = (ushort)lastFolderID;
            lastFolderID++;
            system.files.Add(arm9);

            sFile arm7 = new sFile();
            arm7.name = "arm7.bin";
            arm7.offset = header.ARM7romOffset;
            arm7.size = header.ARM7size;
            arm7.id = (ushort)lastFolderID;
            lastFolderID++;
            system.files.Add(arm7);

            if (header.ARM9overlaySize != 0)
            {
                sFile y9 = new sFile();
                y9.name = "y9.bin";
                y9.offset = header.ARM9overlayOffset;
                y9.size = header.ARM9overlaySize;
                y9.id = (ushort)lastFolderID;
                lastFolderID++;
                system.files.Add(y9);
            }

            if (header.ARM7overlaySize != 0)
            {
                sFile y7 = new sFile();
                y7.name = "y7.bin";
                y7.offset = header.ARM7overlayOffset;
                y7.size = header.ARM7overlaySize;
                y7.id = (ushort)lastFolderID;
                lastFolderID++;
                system.files.Add(y7);
            }

            return system;
        }

        public static sFolder MakeFolderStructure(List<MainFNT> tables, int idFolder, string nameFolder)
        {
            sFolder currFolder = new sFolder();

            currFolder.name = nameFolder;
            currFolder.id = (ushort)idFolder;
            currFolder.files = tables[idFolder & 0xFFF].subTable.files;

            if (tables[idFolder & 0xFFF].subTable.folders is List<sFolder>)
            {
                currFolder.folders = new List<sFolder>();

                foreach (sFolder subFolder in tables[idFolder & 0xFFF].subTable.folders)
                    currFolder.folders.Add(MakeFolderStructure(tables, subFolder.id, subFolder.name));
            }

            return currFolder;
        }

        public static sFile[] ReadBasicOverlays(Stream input, uint offset, uint size, bool arm9, sFAT[] fat)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                sFile[] overlays = new sFile[size / 0x20];

                br.BaseStream.Position = offset;

                for (int i = 0; i < overlays.Length; i++)
                {
                    overlays[i] = new sFile();
                    overlays[i].name = "overlay" + (arm9 ? '9' : '7') + '_' + br.ReadUInt32() + ".bin";
                    br.ReadBytes(20);
                    overlays[i].id = (ushort)br.ReadUInt32();
                    br.ReadBytes(4);
                    overlays[i].offset = fat[overlays[i].id].offset;
                    overlays[i].size = fat[overlays[i].id].size;

                }

                return overlays;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class RomHeader
    {
        public RomHeader(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                gameTitle = br.ReadString(0xc);
                gameCode = br.ReadString(4);
                makerCode = br.ReadString(2);
                unitCode = br.ReadByte();
                encryptionSeed = br.ReadByte();
                size = (uint)Math.Pow(2, 17 + br.ReadByte());
                reserved = br.ReadBytes(9);
                ROMversion = br.ReadByte();
                internalFlags = br.ReadByte();

                ARM9romOffset = br.ReadUInt32();
                ARM9entryAddress = br.ReadUInt32();
                ARM9ramAddress = br.ReadUInt32();
                ARM9size = br.ReadUInt32();

                ARM7romOffset = br.ReadUInt32();
                ARM7entryAddress = br.ReadUInt32();
                ARM7ramAddress = br.ReadUInt32();
                ARM7size = br.ReadUInt32();

                fileNameTableOffset = br.ReadUInt32();
                fileNameTableSize = br.ReadUInt32();

                FAToffset = br.ReadUInt32();
                FATsize = br.ReadUInt32();

                ARM9overlayOffset = br.ReadUInt32();
                ARM9overlaySize = br.ReadUInt32();

                ARM7overlayOffset = br.ReadUInt32();
                ARM7overlaySize = br.ReadUInt32();

                flagsRead = br.ReadUInt32();
                flagsInit = br.ReadUInt32();

                bannerOffset = br.ReadUInt32();
                secureCRC16 = br.ReadUInt16();
                ROMtimeout = br.ReadUInt16();

                ARM9autoload = br.ReadUInt32();
                ARM7autoload = br.ReadUInt32();
                secureDisable = br.ReadUInt64();

                ROMsize = br.ReadUInt32();
                headerSize = br.ReadUInt32();

                reserved2 = br.ReadBytes(0x38);

                br.BaseStream.Position += 0x9c; //nds.logo = br.ReadBytes(156);
                logoCRC16 = br.ReadUInt16();
                headerCRC16 = br.ReadUInt16();

                debug_romOffset = br.ReadUInt32();
                debug_size = br.ReadUInt32();
                debug_ramAddress = br.ReadUInt32();
                reserved3 = br.ReadUInt32();
            }
        }

        public string gameTitle;
        public string gameCode;
        public string makerCode;
        public byte unitCode;
        public byte encryptionSeed;
        public uint size;
        public byte[] reserved;
        public byte ROMversion;
        public byte internalFlags;
        public uint ARM9romOffset;
        public uint ARM9entryAddress;
        public uint ARM9ramAddress;
        public uint ARM9size;
        public uint ARM7romOffset;
        public uint ARM7entryAddress;
        public uint ARM7ramAddress;
        public uint ARM7size;
        public uint fileNameTableOffset;
        public uint fileNameTableSize;
        public uint FAToffset;          // File Allocation Table offset
        public uint FATsize;            // File Allocation Table size
        public uint ARM9overlayOffset;  // ARM9 overlay file offset
        public uint ARM9overlaySize;
        public uint ARM7overlayOffset;
        public uint ARM7overlaySize;
        public uint flagsRead;          // Control register flags for read
        public uint flagsInit;          // Control register flags for init
        public uint bannerOffset;       // Icon + titles offset
        public ushort secureCRC16;      // Secure area CRC16 0x4000 - 0x7FFF
        public ushort ROMtimeout;
        public uint ARM9autoload;
        public uint ARM7autoload;
        public ulong secureDisable;     // Magic number for unencrypted mode
        public uint ROMsize;
        public uint headerSize;
        public byte[] reserved2;        // 56 bytes
        public ushort logoCRC16;
        public ushort headerCRC16;
        public bool secureCRC;
        public bool logoCRC;
        public bool headerCRC;
        public uint debug_romOffset;    // only if debug
        public uint debug_size;         // version with
        public uint debug_ramAddress;   // 0 = none, SIO and 8 MB
        public uint reserved3;          // Zero filled transfered and stored but not used
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Banner
    {
        public Banner(Stream input, uint bannerOffset)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                br.BaseStream.Position = bannerOffset;

                version = br.ReadUInt16();
                CRC16 = br.ReadUInt16();

                reserved = br.ReadBytes(0x1c);
                tileData = br.ReadBytes(0x200);
                palette = br.ReadBytes(0x20);

                var enc = Encoding.GetEncoding("unicode");
                japaneseTitle = enc.GetString(br.ReadBytes(0x100));
                englishTitle = enc.GetString(br.ReadBytes(0x100));
                frenchTitle = enc.GetString(br.ReadBytes(0x100));
                germanTitle = enc.GetString(br.ReadBytes(0x100));
                italianTitle = enc.GetString(br.ReadBytes(0x100));
                spanishTitle = enc.GetString(br.ReadBytes(0x100));
            }
        }

        public ushort version;      // Always 1
        public ushort CRC16;        // CRC-16 of structure, not including first 32 bytes
        public byte[] reserved;     // 28 bytes
        public byte[] tileData;     // 512 bytes
        public byte[] palette;      // 32 bytes
        public string japaneseTitle;// 256 bytes
        public string englishTitle; // 256 bytes
        public string frenchTitle;  // 256 bytes
        public string germanTitle;  // 256 bytes
        public string italianTitle; // 256 bytes
        public string spanishTitle; // 256 bytes
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct sFAT
    {
        public uint offset;
        public uint size;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct sFile
    {
        public uint offset;           // Offset where the files inside of the file in path
        public uint size;             // Length of the file
        public string name;             // File name
        public ushort id;               // Internal id
        public Object tag;              // Extra information
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct sFolder
    {
        public List<sFile> files;           // List of files
        public List<sFolder> folders;      // List of folders
        public string name;                // Folder name
        public ushort id;                  // Internal id
        public Object tag;                 // Extra information
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MainFNT
    {
        public UInt32 offset;
        public UInt16 idFirstFile;
        public UInt16 idParentFolder;
        public sFolder subTable;
    }
}
