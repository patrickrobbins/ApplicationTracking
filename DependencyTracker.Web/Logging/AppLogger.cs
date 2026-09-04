using System;
using System.IO;
using System.Web.Hosting;

namespace DependencyTracker.Web.Logging
{
    /// <summary>
    /// Minimal application logging for unhandled exceptions. Writes to
    /// App_Data\Logs\error.log so failures are recorded without a third-party
    /// logging dependency or elevated event-log permissions.
    /// </summary>
    public static class AppLogger
    {
        private static readonly object SyncRoot = new object();

        public static void LogError(Exception exception, string context = null)
        {
            try
            {
                if (exception == null)
                    return;

                var entry = new System.Text.StringBuilder();
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

                var path = ResolveLogPath();
                if (path == null)
                    return;

                lock (SyncRoot)
                {
                    File.AppendAllText(path, entry.ToString());
                }
            }
            catch
            {
                // Logging must never break the request.
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
