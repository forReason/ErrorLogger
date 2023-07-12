using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace UnhandledExceptionLogger
{
    public partial class Logger
    {
        /// <summary>
        /// Logs an exception to the log file.
        /// </summary>
        /// <param name="severity">The severity of the log entry.</param>
        /// <param name="title">The title of the log entry.</param>
        /// <param name="message">The message of the log entry.</param>
        /// <param name="stackTrace">The stack trace of the exception, if any.</param>
        /// <param name="time">The time the exception occurred. If null, the current time will be used.</param>
        private void AppendRecordToLog(Severity severity, string title, string message, string? stackTrace = null, DateTime? time = null)
        {
            if (time == null)
            {
                time = DateTime.Now;
            }
            lock (LogLock)
            {
                // add new message
                LogHistory.AppendRecord(
                    new[]
                    {
                        (time ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"),
                        severity.ToString(),
                        title,
                        message,
                        stackTrace ?? ""
                    }
                );
                // delete old log entries if requested
                if (HistoryDuration != null || MaxLogSize_KB != null)
                {
                    long logSize = 0;
                    for (int i = LogHistory.Length - 1; i >= 0; i--)
                    {
                        if (HistoryDuration != null)
                        {
                            DateTime logTime = DateTime.ParseExact(LogHistory.GetCell(i, 0), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                            if (logTime < DateTime.Now - HistoryDuration)
                            {
                                TruncateLog(i);
                                break;
                            }
                        }
                        if (MaxLogSize_KB != null)
                        {
                            logSize += Encoding.UTF8.GetBytes(string.Join(' ', LogHistory.GetRecord(i))).Length;
                            if (logSize / 1024 > MaxLogSize_KB)
                            {
                                TruncateLog(i);
                                break;
                            }
                        }
                    }
                }
                // write Log
                try
                {
                    LogHistory.WriteTableToFile(LogPath.FullName);
                }
                catch (Exception ex)
                {
                    { }
                    Debug.WriteLine($"{DateTime.Now.ToString()}WARNING: Could not write Logfile!{Environment.NewLine}" +
                                    $"Exception Message: {ex.Message}{Environment.NewLine}");
                }
            }
        }
    }
}
