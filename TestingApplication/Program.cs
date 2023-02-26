using ErrorLogger;

var logger = new Logger(
                filePath: "C:\\logs\\error.csv",
                historyDuration: TimeSpan.FromDays(30.0),
                maxLogSize_KB: 1024
                );
// Act
throw new DataMisalignedException("the data is misaligned!");