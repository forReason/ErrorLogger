# UnhandledExceptionLogger
A simplistic logger that hooks up to unhandled exceptions and logs them to a specified CSV file.

# Installation
To install ErrorLogger, simply add the NuGet package to your project.

# Usage
To start using ErrorLogger, initialize the logger in your main method:

```
var logger = new Logger();
```

You can also specify the log file path, the duration of the log history to keep, and the maximum size of the log file in kilobytes (KB) by using the following code:
```
var logger = new Logger(filePath, historyDuration, maxLogSize_KB,logLevel,debugOutLevel);
```

The logger should then pick up eceptions automatically. Note there are some known issues when working with multiple threads/tasks/contexts to pick up the exceptions.

Furthermore you can log exceptions and messages manually (for example for debugging or to log handeled exceptions):
```
try
{
    // Some code that may throw an exception...
    int result = int.Parse("not a number");
}
catch (Exception ex)
{
    logger.Log(ex, Severity.Critical);
}
```
```
logger.Log("Application Start", "The application started successfully.", Severity.Info);
```

# Example
Here's an example of how you can use ErrorLogger in your project:

```
using ErrorLogger;

namespace MyProject
{
    class Program
    {
        static void Main(string[] args)
        {
            // initialize logger
            var logger = new Logger(
                filePath: "C:\\logs\\error.csv", 
                historyDuration: TimeSpan.FromDays(30.0), 
                maxLogSize_KB: 1024
                );
            // unhandled exception occurs, application crashes but exception gets logged
            throw new Exception("Test exception");
        }
    }
}
```
In the example above, the logger is initialized with the file path "C:\logs\error.csv", a history duration of 30 days, and a maximum log size of 1024 KB.
The log file is roll over, old entries older than the specified duration are deleted. The same is true for the logfile size.

If no parameters are specified, the logger loggs to the application directory into a file `errors.log` with infinite size.

Note that there can be issues in multithreaded applications for the logger to pick up the exceptions.

# sample output
| Date and Time | Exception Type | Description | Location |
| --- | --- | --- | --- |
| 2/26/2023 21:57 | DataMisalignedException | data is incorrect! | at Program.<Main>$(String[] args) in C:\Users\julia\OneDrive\Projects\Libraries\ErrorLogger\TestingApplication\Program.cs:line 9 |
| 2/26/2023 21:58 | Exception | this is a test exeption! | at Program.<Main>$(String[] args) in C:\Users\julia\OneDrive\Projects\Libraries\ErrorLogger\TestingApplication\Program.cs:line 9 |
| 2/26/2023 21:58 | DataMisalignedException | the data is misaligned! | at Program.<Main>$(String[] args) in C:\Users\julia\OneDrive\Projects\Libraries\ErrorLogger\TestingApplication\Program.cs:line 9 |




# License
ErrorLogger is released under the `MIT-MODERN-VARIANT` license.


