namespace UnhandledExceptionLogger;

/// <summary>
/// defines one log entry
/// </summary>
public struct LogEntry
{
    /// <summary>
    /// Represents one Record in the Log
    /// </summary>
    /// <param name="severity">defines how serious the log entry is</param>
    /// <param name="title">a title message for the log entry</param>
    /// <param name="message">the descriptive message to be conveyed</param>
    /// <param name="time">the recorded time of the log entry</param>
    /// <param name="stackTrace">if the log entry is related to an exception, the Stack trace comes here</param>
    public LogEntry(Logger.Severity severity, string title, string message, DateTime time, string? stackTrace = null)
    {
        Severity = severity;
        Title = title;
        Message = message;
        StackTrace = stackTrace;
        Time = time;
    }

    /// <summary>
    /// defines how serious the log entry is
    /// </summary>
    public Logger.Severity Severity { get; private set; }
    /// <summary>
    /// a title message for the log entry
    /// </summary>
    public string Title { get; private set; }
    /// <summary>
    /// the descriptive message to be conveyed
    /// </summary>
    public string Message { get; private set; }
    /// <summary>
    /// if the log entry is related to an exception, the Stack trace comes here
    /// </summary>
    public string? StackTrace { get; private set; }
    /// <summary>
    /// the recorded time of the log entry
    /// </summary>
    public DateTime Time { get; private set; }

}