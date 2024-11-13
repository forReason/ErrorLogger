namespace UnhandledExceptionLogger;

public partial class Logger
{
    /// <summary>
    /// Specifies the level of severity for a log event.
    /// </summary>
    public enum Severity
    {
        /// <summary>
        /// Informational message that highlights the progress of the application at a coarse-grained level.
        /// </summary>
        Info,

        /// <summary>
        /// Potentially harmful situation where the application is still running as expected.
        /// </summary>
        Warning,

        /// <summary>
        /// Severe error event that will likely lead the application to abort.
        /// </summary>
        Critical
    }

}