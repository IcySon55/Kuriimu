using System.Collections.Generic;
using System.Runtime.InteropServices;
using Kontract;

namespace text_td3
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class Header
    {
        public byte PreMagic; // 0x1B
        public Magic Magic; // LuaQ (5.1)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x17)]
        public byte[] Block;
        public int InstructionCount;
    }

    public sealed class Method
    {
        public string ID;
        public byte Type;
        public uint Length;
        public string Content;

        //public List<Method> Parameters = new List<Method>();
    }
}
