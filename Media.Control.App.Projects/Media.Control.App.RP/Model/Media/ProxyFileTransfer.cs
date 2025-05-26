using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using Media.Control.App.RP.Model.Logger;
using Media.Control.App.RP.Model;
using Media.Control.App.RP.Model.Config;

namespace Media.Control.App.RP.Model
{
    internal class ProxyFileTransfer
    {
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }  
        private long lastCopiedLength = 0;
        private CancellationTokenSource cancelToken = new();
        private DateTime lastCopyTime = DateTime.Now;
        private Logger.Logger logger = new Logger.Logger();

        public  ProxyFileTransfer(string source ="", string target = "" )
        {
            SourcePath = source;
            TargetPath = target;

        }

        public async Task StartWatchingAsync(int intervalMilliseconds = 1000)
        {

            var watchingTask = Task.Run(async () =>
            {
                while (!cancelToken.Token.IsCancellationRequested)
                {
                    try
                    {
                        FileInfo sourceFile = new FileInfo(SourcePath);
                        if (sourceFile.Exists)
                        {
                            long currentSize = sourceFile.Length;
                            if (currentSize > lastCopiedLength)
                            {
                                Debug.WriteLine($"[INFO] Copying {currentSize - lastCopiedLength} bytes...");
                                await CopyDeltaAsync(currentSize);
                                lastCopyTime = DateTime.Now;
                            }
                            else if ((DateTime.Now - lastCopyTime).TotalSeconds > 3)
                            {
                                Debug.WriteLine("[INFO] No file growth detected for over 3 seconds. Stopping...");
                                StopWatching();
                                break;
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        Debug.WriteLine("[ERROR] File access error: " + ex.Message);
                        //logger.Log("Error", "File Access Error", ex.Message, ex.ToString());
                    }

                    await Task.Delay(intervalMilliseconds);
                }
            });

            await watchingTask;
        }

        public void StopWatching() => cancelToken.Cancel();

        public async ValueTask DisposeAsync()
        {
            if (cancelToken != null)
            {
                cancelToken.Cancel();
                cancelToken.Dispose();
                cancelToken = null;
            }

            await Task.CompletedTask;
        }

        private async Task CopyDeltaAsync(long currentSize)
        {
            using (var sourceStream = new FileStream(SourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var targetStream = new FileStream(TargetPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                sourceStream.Seek(lastCopiedLength, SeekOrigin.Begin);
                targetStream.Seek(lastCopiedLength, SeekOrigin.Begin);

                byte[] buffer = new byte[8192];
                int bytesRead;

                while (sourceStream.Position < currentSize &&
                       (bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await targetStream.WriteAsync(buffer, 0, bytesRead);
                }

                lastCopiedLength = currentSize;
            }
        }

    }
}
