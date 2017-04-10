using System;
using System.Runtime.InteropServices;

namespace Kuriimu.IO
{
    public static class Extensions
    {
        public static unsafe T ToStruct<T>(this byte[] buffer, int offset = 0)
        {
            fixed (byte* pBuffer = buffer)
                return Marshal.PtrToStructure<T>((IntPtr)pBuffer + offset);
        }

        public static unsafe byte[] StructToArray<T>(this T item)
        {
            var buffer = new byte[Marshal.SizeOf(typeof(T))];
            fixed (byte* pBuffer = buffer)
            {
                Marshal.StructureToPtr(item, (IntPtr)pBuffer, false);
            }
            return buffer;
        }
    }
}
