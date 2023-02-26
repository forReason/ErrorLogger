using System.Text;

namespace ErrorLogger
{
    /// <summary>
    /// The Logger class is a utility for logging errors and exceptions to a file. 
    /// It provides options for limiting the size of the log file and the duration of the log history.
    /// When an unhandled exception occurs in the application, the Logger class automatically logs the exception details to the log file. 
    /// The log file is stored in a QuickCsv.Net Table object, which allows for easy reading and writing of log data. 
    /// The Logger class is designed to be easily integrated into any .NET application, providing a simple and robust logging solution.
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// The duration of the log history to keep. Old log entries will be deleted.
        /// </summary>
        public TimeSpan? HistoryDuration { get; set; }

        /// <summary>
        /// The maximum size of the log file in kilobytes (KB). If the log file exceeds this size, old log entries will be deleted.
        /// </summary>
        public long? MaxLogSize_KB { get; set; }

        /// <summary>
        /// The log data as a QuickCsv.Net.Table_NS.Table object.
        /// </summary>
        public QuickCsv.Net.Table_NS.Table Log { get; set; } = new QuickCsv.Net.Table_NS.Table();

        /// <summary>
        /// the lock making sure that the file is only beeing accessed once
        /// </summary>
        object LogLock = new object();

        /// <summary>
        /// the file location where the log should be written to (note, the directory should exist)
        /// </summary>
        FileInfo LogPath { get; set; }

        /// <summary>
        /// Creates a new instance of the Logger class.
        /// </summary>
        /// <param name="filePath">The path to the log file.</param>
        /// <param name="historyDuration">The duration of the log history to keep. Old log entries will be deleted.</param>
        /// <param name="maxLogSize_KB">The maximum size of the log file in kilobytes (KB). If the log file exceeds this size, old log entries will be deleted.</param>
        public Logger(string filePath = "errors.log", TimeSpan? historyDuration = null, long? maxLogSize_KB = null)
        {
            // convert path to fileinfo
            LogPath =  new FileInfo(filePath);
            // make sure the directory structure exists
            if (!Directory.Exists(LogPath.DirectoryName))
            {
                Directory.CreateDirectory(LogPath.DirectoryName);
            }
            // if the log file exists, load it into the table. If it doesnt, create a new table
            if (LogPath.Exists)
            {
                Log.LoadFromFile(LogPath.FullName,hasHeaders: true);       
            }
            else
            {
                Log.SetColumnNames(new[] { "Timestamp", "Exception Type", "Message", "Stack Trace" });
            }
            // set variables
            this.HistoryDuration = historyDuration;
            this.MaxLogSize_KB = maxLogSize_KB;
            // register event handler of uncaught exceptions in order to log tem in errors.log
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                // Log the exception details
                LogException(ex);
            };
        }
        /// <summary>
        /// removes the oldest entries of the log, up to the index i
        /// </summary>
        /// <param name="i"></param>
        private void TruncateLog(int i)
        {
            for (int r = i; r >= 0; r--)
            {
                Log.RemoveRecord(r);
            }
        }
        /// <summary>
        /// Logs an exception to the log file.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        public void LogException(Exception ex)
        {
            lock (LogLock)
            {
                // add new message
                Log.AppendRecord(
                    new[]
                    {
                        DateTime.Now.ToString(),
                        ex.GetType().Name,
                        ex.Message,
                        ex.StackTrace
                    }
                );
                // delete old log entries if requested
                if (HistoryDuration != null || MaxLogSize_KB != null)
                {
                    long logSize = 0;
                    for (int i = Log.Length-1; i >= 0; i--)
                    {   
                        if (HistoryDuration != null)
                        {
                            DateTime time = DateTime.Parse(Log.GetCell(i, 0));
                            if (time < DateTime.Now-HistoryDuration)
                            {
                                TruncateLog(i);
                                break;
                            }
                        }
                        if (MaxLogSize_KB != null)
                        {
                            logSize += Encoding.UTF8.GetBytes(string.Join(' ' , Log.GetRecord(i))).Length;
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
                    Log.WriteTableToFile(LogPath.FullName);
                }
                catch (Exception ox)
                {
                    { }
                }
            }
        }
    }
}