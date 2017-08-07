using System.Runtime.InteropServices;
using System.Collections.Generic;
using Kuriimu.Kontract;
using System;
using System.IO;
using Kuriimu.IO;
using Cetera.Image;
using System.Drawing;

namespace image_nintendo.BCH
{
    public class Header
    {
        public Header(Stream input)
        {
            using (var br = new BinaryReaderX(input, true))
            {
                magic = br.ReadStruct<Magic>();
                backwardComp = br.ReadByte();
                forwardComp = br.ReadByte();
                version = br.ReadUInt16();

                mainHeaderOffset = br.ReadUInt32();
                nameTableOffset = br.ReadUInt32();
                gpuCommandsOffset = br.ReadUInt32();
                dataOffset = br.ReadUInt32();
                if (backwardComp > 0x20) dataExtOffset = br.ReadUInt32();
                relocTableOffset = br.ReadUInt32();

                mainHeaderSize = br.ReadUInt32();
                nameTableSize = br.ReadUInt32();
                gpuCommandsSize = br.ReadUInt32();
                dataSize = br.ReadUInt32();
                if (backwardComp > 0x20) dataExtSize = br.ReadUInt32();
                relocTableSize = br.ReadUInt32();

                uninitDataSectionSize = br.ReadUInt32();
                uninitDescSectionSize = br.ReadUInt32();

                if (backwardComp > 7)
                {
                    flags = br.ReadUInt16();
                    addressCount = br.ReadUInt16();
                }
            }
        }

        public Magic magic;
        public byte backwardComp;
        public byte forwardComp;
        public ushort version;

        public uint mainHeaderOffset;
        public uint nameTableOffset;
        public uint gpuCommandsOffset;
        public uint dataOffset;
        public uint dataExtOffset = 0;
        public uint relocTableOffset;

        public uint mainHeaderSize;
        public uint nameTableSize;
        public uint gpuCommandsSize;
        public uint dataSize;
        public uint dataExtSize = 0;
        public uint relocTableSize;

        public uint uninitDataSectionSize;
        public uint uninitDescSectionSize;

        public ushort flags;
        public ushort addressCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MainHeader
    {
        public uint modelsPointerTableOffset;
        public uint modelsPointerTableEntries;
        public uint modelsNameOffset;
        public uint matPointerTableOffset;
        public uint matPointerTableEntries;
        public uint matNameOffset;
        public uint shadersPointerTableOffset;
        public uint shadersPointerTableEntries;
        public uint shadersNameOffset;
        public uint texPointerTableOffset;
        public uint texPointerTableEntries;
        public uint texturesNameOffset;
        public uint matLUTPointerTableOffset;
        public uint matLUTPointerTableEntries;
        public uint matLUTNameOffset;
        public uint lightsPointerTableOffset;
        public uint lightsPointerTableEntries;
        public uint lightsNameOffset;
        public uint camPointerTableOffset;
        public uint camPointerTableEntries;
        public uint camNameOffset;
        public uint fogsPointerTableOffset;
        public uint fogsPointerTableEntries;
        public uint fogsNameOffset;
        public uint skeletalAnimationsPointerTableOffset;
        public uint skeletalAnimationsPointerTableEntries;
        public uint skeletalAnimationsNameOffset;
        public uint materialAnimationsPointerTableOffset;
        public uint materialAnimationsPointerTableEntries;
        public uint materialAnimationsNameOffset;
        public uint visibilityAnimationsPointerTableOffset;
        public uint visibilityAnimationsPointerTableEntries;
        public uint visibilityAnimationsNameOffset;
        public uint lightAnimationsPointerTableOffset;
        public uint lightAnimationsPointerTableEntries;
        public uint lightAnimationsNameOffset;
        public uint cameraAnimationsPointerTableOffset;
        public uint cameraAnimationsPointerTableEntries;
        public uint cameraAnimationsNameOffset;
        public uint fogAnimationsPointerTableOffset;
        public uint fogAnimationsPointerTableEntries;
        public uint fogAnimationsNameOffset;
        public uint scenePointerTableOffset;
        public uint scenePointerTableEntries;
        public uint sceneNameOffset;
    }

    public class TexEntry
    {
        public TexEntry(int width, int height, Format format)
        {
            this.width = width;
            this.height = height;
            this.format = format;
        }

        public int width;
        public int height;
        public Format format;
    }
}
