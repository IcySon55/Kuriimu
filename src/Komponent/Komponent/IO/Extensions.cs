using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;

namespace Komponent.IO
{
    [DebuggerDisplay("{(string)this}")]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Magic
    {
        int value;
        public static implicit operator string(Magic magic) => Encoding.ASCII.GetString(BitConverter.GetBytes(magic.value));
        public static implicit operator Magic(string s) => new Magic { value = BitConverter.ToInt32(Encoding.ASCII.GetBytes(s), 0) };
    }

    [DebuggerDisplay("{(string)this}")]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Magic8
    {
        long value;
        public static implicit operator string(Magic8 magic) => Encoding.ASCII.GetString(BitConverter.GetBytes(magic.value));
    }

    public enum ByteOrder : ushort
    {
        LittleEndian = 0xFEFF,
        BigEndian = 0xFFFE
    }

    public static class Extensions
    {
        // Read
        public static unsafe T BytesToStruct<T>(this byte[] buffer, ByteOrder byteOrder = ByteOrder.LittleEndian, int offset = 0)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            
            AdjustByteOrder(typeof(T), buffer, byteOrder);

            fixed (byte* pBuffer = buffer)
                return Marshal.PtrToStructure<T>((IntPtr)pBuffer + offset);
        }

        public static unsafe T BytesToStruct<T>(this byte[] buffer, int offset) => BytesToStruct<T>(buffer, ByteOrder.LittleEndian, offset);

        // Write
        public static unsafe byte[] StructToBytes<T>(this T item, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            var buffer = new byte[Marshal.SizeOf(typeof(T))];

            fixed (byte* pBuffer = buffer)
                Marshal.StructureToPtr(item, (IntPtr)pBuffer, false);

            AdjustByteOrder(typeof(T), buffer, byteOrder);

            return buffer;
        }

        // BigEndian Support
        private static void AdjustByteOrder(Type type, byte[] buffer, ByteOrder byteOrder, int startOffset = 0)
        {
            if (BitConverter.IsLittleEndian == (byteOrder == ByteOrder.LittleEndian)) return;

            if (type.IsPrimitive)
            {
                if (type == typeof(short) || type == typeof(ushort) ||
                    type == typeof(int) || type == typeof(uint) ||
                    type == typeof(long) || type == typeof(ulong))
                {
                    Array.Reverse(buffer);
                    return;
                }
            }

            foreach (var field in type.GetFields())
            {
                var fieldType = field.FieldType;

                // Ignore static fields
                if (field.IsStatic) continue;

                if (fieldType.BaseType == typeof(Enum) && fieldType != typeof(ByteOrder))
                    fieldType = fieldType.GetFields()[0].FieldType;

                // Swap bytes only for the following types (incomplete just like BinaryReaderX is)
                if (fieldType == typeof(short) || fieldType == typeof(ushort) ||
                    fieldType == typeof(int) || fieldType == typeof(uint) ||
                    fieldType == typeof(long) || fieldType == typeof(ulong))
                {
                    var offset = Marshal.OffsetOf(type, field.Name).ToInt32();

                    // Enums
                    if (fieldType.IsEnum)
                        fieldType = Enum.GetUnderlyingType(fieldType);

                    // Check for sub-fields to recurse if necessary
                    var subFields = fieldType.GetFields().Where(subField => subField.IsStatic == false).ToArray();
                    var effectiveOffset = startOffset + offset;

                    if (subFields.Length == 0)
                        Array.Reverse(buffer, effectiveOffset, Marshal.SizeOf(fieldType));
                    else
                        AdjustByteOrder(fieldType, buffer, byteOrder, effectiveOffset);
                }
            }
        }
    }
}
