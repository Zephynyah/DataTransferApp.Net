using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DataTransferApp.Utils
{
    public static class WinDefender
    {
        private static bool _isDefenderAvailable;
        private static string? _defenderPath;
        private static SemaphoreSlim _lock = new SemaphoreSlim(5); // limit to 5 concurrent checks at a time

        // static ctor
        static WinDefender()
        {
            if (OperatingSystem.IsWindows())
            {
                _defenderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows Defender", "MpCmdRun.exe");
                _isDefenderAvailable = File.Exists(_defenderPath);
            }
            else
            {
                _isDefenderAvailable = false;
            }
        }

        public static async Task<bool> IsVirus(byte[] file, CancellationToken cancellationToken = default)
        {
            if (!_isDefenderAvailable)
                return false;

            string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            await File.WriteAllBytesAsync(path, file, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return false;

            try
            {
                return await IsVirus(path, cancellationToken);
            }
            finally
            {
                File.Delete(path);
            }
        }

        public static async Task<bool> IsVirus(string path, CancellationToken cancellationToken = default)
        {
            if (!_isDefenderAvailable || _defenderPath == null)
                return false;

            await _lock.WaitAsync(cancellationToken);

            try
            {
                using (var process = Process.Start(_defenderPath, $"-Scan -ScanType 3 -File \"{path}\" -DisableRemediation"))
                {
                    if (process == null)
                    {
                        // disable future attempts
                        _isDefenderAvailable = false; 
                        throw new InvalidOperationException("Failed to start MpCmdRun.exe");
                    }

                    try
                    {
                        await process.WaitForExitAsync().WaitAsync(TimeSpan.FromMilliseconds(2500), cancellationToken);
                    }
                    catch (TimeoutException ex) // timeout
                    {
                        throw new TimeoutException("Timeout waiting for MpCmdRun.exe to return", ex);
                    }
                    finally
                    {
                        process.Kill(); // always kill the process, it's fine if it's already exited, but if we were timed out or cancelled via token - let's kill it
                    }

                    return process.ExitCode == 2;
                }
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
