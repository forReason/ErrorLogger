using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace UnhandledExceptionLogger;

public partial class Logger
{
    /// <summary>
    /// the queue for adding elements, while a logging task is running
    /// </summary>
    private readonly ConcurrentQueue<LogEntry> _loggingQueue = new();

    /// <summary>
    /// the lock making sure that the file is only being accessed once
    /// </summary>
    private readonly object _logLock = new();

    /// <summary>
    /// defines if a logging process is currently running
    /// </summary>
    private volatile bool _isWriting;

    /// <summary>
    /// Logs an exception to the log file.
    /// </summary>
    /// <remarks>
    /// enqueues the log entry and spins up a background task for writing the Log file.
    /// The Logfile is being truncated by the specified date and size limit automatically
    /// </remarks>
    /// <param name="severity">The severity of the log entry.</param>
    /// <param name="title">The title of the log entry.</param>
    /// <param name="message">The message of the log entry.</param>
    /// <param name="stackTrace">The stack trace of the exception, if any.</param>
    /// <param name="time">The time the exception occurred. If null, the current time will be used.</param>
    private void AppendRecordToLog(Severity severity, string title, string message, string? stackTrace = null,
        DateTime? time = null)
    {
        time ??= DateTime.Now;
        while (_loggingQueue.Count > 10000)
        {
            Thread.Sleep(100);
        }
        _loggingQueue.Enqueue(new LogEntry(severity, title, message, time.Value, stackTrace));
        Task.Run(WriteLog).ConfigureAwait(false);
    }

    /// <summary>
    /// writes to the Logfile asynchronously
    /// </summary>
    private async Task WriteLog()
    {
        while (true)
        {
            lock (_logLock)
            {
                if (_isWriting) return;
                _isWriting = true;
            }

            try
            {
                while (_loggingQueue.TryDequeue(out LogEntry logEntry))
                {
                    // add new message
                    LogHistory.AppendRecord(new[] { logEntry.Time.ToString(TimeFormat), logEntry.Severity.ToString(), logEntry.Title, logEntry.Message, logEntry.StackTrace ?? "" });
                }
                // delete old log entries if requested
                if (HistoryDuration is not null && MaxLogSize_KB is not null)
                {
                    long logSize = 0;
                    for (int i = LogHistory.Length - 1; i >= 0; i--)
                    {
                        if (HistoryDuration is not null)
                        {
                            DateTime logTime = DateTime.ParseExact(LogHistory.GetCell(i, 0), TimeFormat, CultureInfo.InvariantCulture);
                            if (logTime < DateTime.Now - HistoryDuration)
                            {
                                TruncateLog(i);
                                break;
                            }
                        }

                        if (MaxLogSize_KB is null) continue;
                        logSize += Encoding.UTF8.GetBytes(string.Join(' ', LogHistory.GetRecord(i))).Length;
                        if (!(logSize / 1024 > MaxLogSize_KB)) continue;
                        TruncateLog(i);
                        break;
                    }
                }
                

                // write Log
                try
                {
                    LogHistory.WriteTableToFile(LogPath.FullName);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        $"{DateTime.Now.ToString(TimeFormat)}" +
                        $"WARNING: Could not write Logfile!{Environment.NewLine}" + 
                        $"Exception Message: {ex.Message}{Environment.NewLine}");
                }
            }
            finally
            {
                lock (_logLock)
                {
                    _isWriting = false;
                }
            }

            if (!_loggingQueue.IsEmpty) continue;

            break;
        }

        await Task.CompletedTask;
    }
}
