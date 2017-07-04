using System;
using System.Linq;
using System.Text;

namespace Cetera.Hardware
{
    public class OnionFS
    {
        public static unsafe string DoStuff(byte[] codeBin)
        {
            fixed (byte* bytePtr = codeBin)
            {
                var ptr = (uint*)bytePtr;
                var range = Enumerable.Range(0, codeBin.Length / 4);
                Func<Func<int, bool>, int> FindFunc = pred =>
                {
                    var x = range.Where(pred).Single();
                    while (ptr[--x] >> 16 != 0xE92D)
                    {
                    }
                    return 0x100000 + x * 4;
                };

                var sb = new StringBuilder();
                int mountArchive = FindFunc(i => (ptr[i] == 0xE5970010 && ptr[i + 1] == 0xE1CD20D8 && (ptr[i + 2] & 0xFFFFFF) == 0x8D0000)
                    || (ptr[i] == 0xE24DD028 && ptr[i + 1] == 0xE1A04000 && ptr[i + 2] == 0xE59F60A8 && ptr[i + 3] == 0xE3A0C001));
                sb.AppendLine($"mountArchive equ 0x{mountArchive:X6}");
                int regArchive = FindFunc(i => ptr[i] == 0xC82044B4 && ptr[i + 1] == 0xD8604659);
                sb.AppendLine($"regArchive equ 0x{regArchive:X6}");
                int userFsTryOpen = FindFunc(i => (ptr[i] == 0xE1A0100D || ptr[i] == 0xE28D1010) && ptr[i + 1] == 0xE590C000
                    && (ptr[i + 2] == 0xE1A00004 || ptr[i + 2] == 0xE1A00005) && ptr[i + 3] == 0xE12FFF3C);
                sb.AppendLine($"userFsTryOpen equ 0x{userFsTryOpen:X6}");
                var svc2D = range.Single(i => ptr[i] == 0xEF00002D);
                var throwFatalError = FindFunc(i => (ptr[i] == (0xEB000000 | (uint)(svc2D - i - 3) & 0xFFFFFF))
                    && Enumerable.Range(0, 999).Select(j => ptr[i + j + 1]).TakeWhile(p => (p >> 16) != 0xE92D).All(p => p != 0xE200167E));
                sb.AppendLine($"throwFatalError equ 0x{throwFatalError:X6}");

                //var patchOffset = range.Single(i => ptr[i] == 0xE1A02000 && ptr[i + 1] == 0xE59F1070 && ptr[i + 2] == 0xE59F0064 && ptr[i + 3] == 0xE3A03000);
                //sb.AppendLine($"{ptr[patchOffset + 10]:X8}");

                return sb.ToString();
            }
        }
    }
}
