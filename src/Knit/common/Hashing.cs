using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Knit.Hashing
{
    public static class SHA1Hash
    {
        public static string Create(Stream input) => string.Join("", SHA1.Create().ComputeHash(input).Select(b => b.ToString("X2")));
        public static string Create(byte[] input) => string.Join("", SHA1.Create().ComputeHash(input).Select(b => b.ToString("X2")));
        public static string Create(string input) => string.Join("", SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(input)).Select(b => b.ToString("X2")));
    }

    public static class SHA256Hash
    {
        public static string Create(Stream input) => string.Join("", SHA256.Create().ComputeHash(input).Select(b => b.ToString("X2")));
        public static string Create(byte[] input) => string.Join("", SHA256.Create().ComputeHash(input).Select(b => b.ToString("X2")));
        public static string Create(string input) => string.Join("", SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes(input)).Select(b => b.ToString("X2")));
    }

    public enum HashTypes
    {
        None,
        SHA1,
        SHA256
    }
}
