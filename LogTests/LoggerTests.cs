using UnhandledExceptionLogger;

namespace LogTests
{
    public class LoggerTests : IDisposable
    {
        private readonly string logFilePath = Path.Combine("temp","test_log.log");
        private readonly Logger logger;

        public LoggerTests()
        {
            // Create a new instance of the Logger class for each test
            logger = new Logger(logFilePath);
        }

        //[Fact]
        //public void UnhandledException_WritesExceptionToLog()
        //{
        //    // Arrange
        //    var logger = new Logger();
        //    var ex = new Exception("Test exception");
        //    // Act
        //    throw ex;
        //}

        [Fact]
        public void Logging_Logs()
        {
            logger.Log("testentry","testmessage", Logger.Severity.Critical);
            {}
        }
        
        [Fact]
        public void LogPath_DirectoryExists()
        {
            FileInfo loggerFile = new FileInfo(logFilePath);
            // Assert
            Assert.True(Directory.Exists(loggerFile.DirectoryName));
        }

        public void Dispose()
        {
            // Delete the test log file after all tests have run
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }
        }
    }
}
