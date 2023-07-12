using System.Diagnostics;
using System.Text;

namespace UnhandledExceptionLogger
{
    /// <summary>
    /// The Logger class is a utility for logging errors and exceptions to a file. 
    /// It provides options for limiting the size of the log file and the duration of the log history.
    /// When an unhandled exception occurs in the application, the Logger class automatically logs the exception details to the log file. 
    /// The log file is stored in a QuickCsv.Net Table object, which allows for easy reading and writing of log data. 
    /// The Logger class is designed to be easily integrated into any .NET application, providing a simple and robust logging solution.
    /// </summary>
    public partial class Logger
    {
        /// <summary>
        /// sets the loglevel based on which is decided if a message is logged to file or not
        /// </summary>
        public Severity LogLevel { get; set; } = Severity.Critical;
        /// <summary>
        /// sets the loglevel based on which is decided if a message is printed to console or not
        /// </summary>
        public Severity DebugOutLevel { get; set; } = Severity.Warning;
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
        public QuickCsv.Net.Table_NS.Table LogHistory { get; set; } = new QuickCsv.Net.Table_NS.Table();

        /// <summary>
        /// the lock making sure that the file is only beeing accessed once
        /// </summary>
        readonly object LogLock = new();

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
        /// <param name="logLevel">The minimum severity which should be logged to file.</param>
        /// <param name="debugOutLevel">The minimum severity which should be printed to console.</param>
        public Logger(
            string filePath = "errors.log", 
            TimeSpan? historyDuration = null, 
            long? maxLogSize_KB = null, 
            Severity logLevel = Severity.Critical,
            Severity debugOutLevel = Severity.Warning)
        {
            LogLevel= logLevel;
            DebugOutLevel= debugOutLevel;
            // convert path to fileinfo
            LogPath =  new FileInfo(filePath);
            // make sure the directory structure exists
            string? directoryName = LogPath.DirectoryName;
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            // if the log file exists, load it into the table. If it doesnt, create a new table
            if (LogPath.Exists)
            {
                LogHistory.LoadFromFile(LogPath.FullName,hasHeaders: true);       
            }
            else
            {
                LogHistory.SetColumnNames(new[] { "Timestamp", "Severity" ,"Exception Type", "Message", "Stack Trace" });
            }
            // set variables
            this.HistoryDuration = historyDuration;
            this.MaxLogSize_KB = maxLogSize_KB;
            // register event handler of uncaught exceptions in order to log tem in errors.log
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                // Log the exception details
                Log(ex);
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
                LogHistory.RemoveRecord(r);
            }
        }
        /// <summary>
        /// Logs an exception to the log file.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="exceptionSeverity">the severity of the Exception, defaults to Severity.Critical</param>
        public void Log(Exception ex, Severity exceptionSeverity = Severity.Critical)
        {
            if (exceptionSeverity >= LogLevel)
            {
                AppendRecordToLog(severity: exceptionSeverity, title: ex.GetType().Name, message: ex.Message, stackTrace: ex.StackTrace);
            }
            if (exceptionSeverity >= DebugOutLevel)
            {
                WriteDebug(severity: exceptionSeverity, title: ex.GetType().Name, message: ex.Message);
            }
        }
        /// <summary>
        /// logs a message to console and file
        /// </summary>
        /// <param name="title">the message title</param>
        /// <param name="message">the message</param>
        /// <param name="exceptionSeverity">the severity of the message, defaults to Severity.Info</param>
        public void Log(string title, string message, Severity exceptionSeverity = Severity.Info)
        {
            if (exceptionSeverity >= LogLevel)
            {
                AppendRecordToLog(severity: exceptionSeverity, title: title, message: message);
            }
            if (exceptionSeverity >= DebugOutLevel)
            {
                WriteDebug(severity: exceptionSeverity, title: title, message: message);
            }
        }
        /// <summary>
        /// Writes a debug message with a specific severity, title, message, and time.
        /// </summary>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="title">The title of the message.</param>
        /// <param name="message">The message body.</param>
        /// <param name="time">The time of the message. If null, the current system time will be used.</param>
        private static void WriteDebug(Severity severity, string title, string message, DateTime? time = null)
        {
            string formattedTime = (time ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss");
            Debug.WriteLine($"{formattedTime} {severity}: {title} - {message}");
        }

    }
}