using System;
using System.IO;
using System.Text;
using System.Web.Hosting;

namespace DependencyTracker.Web.Logging
{
    /// <summary>
    /// Minimal application logging for unhandled exceptions and security events.
    /// Writes to App_Data\Logs\error.log so failures are recorded without a
    /// third-party logging dependency or elevated event-log permissions. The
    /// file is rotated once it grows past a fixed size so it cannot fill the disk.
    /// </summary>
    public static class AppLogger
    {
        private const long MaxLogBytes = 5 * 1024 * 1024;
        private const int MaxArchivedFiles = 2;

        private static readonly object SyncRoot = new object();

        public static void LogError(Exception exception, string context = null)
        {
            try
            {
                if (exception == null)
                    return;

                var entry = new StringBuilder();
                entry.AppendLine("------------------------------------------------------------");
                entry.AppendLine("Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                entry.AppendLine("Context: " + (context ?? "unknown"));
                entry.AppendLine("Type: " + exception.GetType().FullName);
                entry.AppendLine("Message: " + exception.Message);
                entry.AppendLine("Stack:");
                entry.AppendLine(exception.StackTrace);

                var inner = exception.InnerException;
                while (inner != null)
                {
                    entry.AppendLine("Inner: " + inner.GetType().FullName + " - " + inner.Message);
                    entry.AppendLine(inner.StackTrace);
                    inner = inner.InnerException;
                }

                entry.AppendLine();
                WriteEntry(entry.ToString());
            }
            catch
            {
                // Logging must never break the request.
            }
        }

        /// <summary>
        /// Records a security relevant event (a denied request, a rejected
        /// anti-forgery token) without an exception and stack trace.
        /// </summary>
        public static void LogSecurityEvent(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                    return;

                var entry = new StringBuilder();
                entry.AppendLine("------------------------------------------------------------");
                entry.AppendLine("Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                entry.AppendLine("Security: " + message);
                entry.AppendLine();
                WriteEntry(entry.ToString());
            }
            catch
            {
                // Logging must never break the request.
            }
        }

        private static void WriteEntry(string entry)
        {
            var path = ResolveLogPath();
            if (path == null)
                return;

            lock (SyncRoot)
            {
                try
                {
                    var info = new FileInfo(path);
                    if (info.Exists && info.Length >= MaxLogBytes)
                        Rotate(path);

                    File.AppendAllText(path, entry);
                }
                catch
                {
                    // Logging must never break the request.
                }
            }
        }

        private static void Rotate(string path)
        {
            var oldest = path + "." + MaxArchivedFiles;
            try
            {
                if (File.Exists(oldest))
                    File.Delete(oldest);
            }
            catch
            {
                // ignore; keep appending to the current file
            }

            for (var i = MaxArchivedFiles - 1; i >= 1; i--)
            {
                var from = path + "." + i;
                var to = path + "." + (i + 1);
                try
                {
                    if (File.Exists(from))
                        File.Move(from, to);
                }
                catch
                {
                    // ignore; keep appending to the current file
                }
            }

            try
            {
                File.Move(path, path + ".1");
            }
            catch
            {
                // ignore; keep appending to the current file
            }
        }

        private static string ResolveLogPath()
        {
            try
            {
                var basePath = HostingEnvironment.MapPath("~/App_Data/Logs");
                if (string.IsNullOrEmpty(basePath))
                    basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Logs");

                Directory.CreateDirectory(basePath);
                return Path.Combine(basePath, "error.log");
            }
            catch
            {
                return null;
            }
        }
    }
}
