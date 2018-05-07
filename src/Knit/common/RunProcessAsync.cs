using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Knit
{
    public class RunProcessAsync
    {
        public event EventHandler Exited;

        public delegate void ExitedEventHandler(object sender, EventArgs e);

        protected virtual void OnExited(EventArgs e)
        {
            var handler = Exited;
            handler?.Invoke(this, e);
        }

        public event EventHandler OutputDataReceived;

        public delegate void OutputDataReceivedEventHandler(object sender, DataReceivedEventArgs e);

        protected virtual void OnOutputDataReceived(DataReceivedEventArgs e)
        {
            var handler = OutputDataReceived;
            handler?.Invoke(this, e);
        }

        public event EventHandler ErrorDataReceived;

        public delegate void ErrorDataReceivedEventHandler(object sender, DataReceivedEventArgs e);

        protected virtual void OnErrorDataReceived(DataReceivedEventArgs e)
        {
            var handler = ErrorDataReceived;
            handler?.Invoke(this, e);
        }

        public Task StartAsync(ProcessStartInfo startInfo)
        {
            var tcs = new TaskCompletionSource<bool>();

            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;

            var process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                OnExited(args);
                tcs.SetResult(true);
                process.Dispose();
            };
            process.OutputDataReceived += (sender, args) =>
            {
                OnOutputDataReceived(args);
            };
            process.ErrorDataReceived += (sender, args) =>
            {
                OnErrorDataReceived(args);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return tcs.Task;
        }
    }
}
