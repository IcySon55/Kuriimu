using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Knit.Hashing
{
    public static class SHA1Hash
    {
        public static async Task<string> ComputeHashAsync(Stream input, IProgress<ProgressReport> progress)
        {
            var sha1 = SHA1.Create();
            var progressReport = new ProgressReport();
            const int bufferSize = 2048;

            sha1.Initialize();

            var buffer = new byte[bufferSize];
            var streamLength = input.Length;
            while (true)
            {
                var read = await input.ReadAsync(buffer, 0, bufferSize).ConfigureAwait(false);
                if (input.Position == streamLength)
                {
                    sha1.TransformFinalBlock(buffer, 0, read);
                    progressReport.Percentage = 100;
                    progress.Report(progressReport);
                    break;
                }
                sha1.TransformBlock(buffer, 0, read, default(byte[]), default(int));
                progressReport.Percentage = (double)input.Position / Math.Max(input.Length, 1) * 100;
                progress.Report(progressReport);
            }

            return string.Join("", sha1.Hash.Select(b => b.ToString("X2")));
        }
    }

    public static class SHA256Hash
    {
        public static async Task<string> ComputeHashAsync(Stream input, IProgress<ProgressReport> progress)
        {
            var sha256 = SHA256.Create();
            var progressReport = new ProgressReport();
            const int bufferSize = 2048;

            sha256.Initialize();

            var buffer = new byte[bufferSize];
            var streamLength = input.Length;
            while (true)
            {
                var read = await input.ReadAsync(buffer, 0, bufferSize).ConfigureAwait(false);
                if (input.Position == streamLength)
                {
                    sha256.TransformFinalBlock(buffer, 0, read);
                    progressReport.Percentage = 100;
                    progress.Report(progressReport);
                    break;
                }
                sha256.TransformBlock(buffer, 0, read, default(byte[]), default(int));
                progressReport.Percentage = (double)input.Position / Math.Max(input.Length, 1) * 100;
                progress.Report(progressReport);
            }

            return string.Join("", sha256.Hash.Select(b => b.ToString("X2")));
        }
    }

    public enum HashTypes
    {
        None,
        SHA1,
        SHA256
    }
}
