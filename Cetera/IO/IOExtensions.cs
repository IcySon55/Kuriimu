using System;
using System.Runtime.InteropServices;

namespace Cetera.IO
{
    public static class IOExtensions
    {
        public static unsafe T ToStruct<T>(this byte[] buffer)
        {
            fixed (byte* pBuffer = buffer)
                return Marshal.PtrToStructure<T>((IntPtr)pBuffer);
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
