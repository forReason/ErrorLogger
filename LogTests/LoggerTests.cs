using UnhandledExceptionLogger;
using System;
using System.IO;
using Xunit;

namespace LogTests
{
    public class LoggerTests : IDisposable
    {
        private readonly string logFilePath = "test_log.log";
        private readonly Logger logger;

        public LoggerTests()
        {
            // Create a new instance of the Logger class for each test
            logger = new Logger(logFilePath);
        }

        [Fact]
        public void UnhandledException_WritesExceptionToLog()
        {
            // Arrange
            var logger = new Logger();
            var ex = new Exception("Test exception");
            // Act
            throw ex;
            throw new Exception("Test exception");
        }

        [Fact]
        public void LogPath_DirectoryExists()
        {
            // Assert
            Assert.True(Directory.Exists(Path.GetDirectoryName(logFilePath)));
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
