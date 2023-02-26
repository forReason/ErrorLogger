using ErrorLogger;

AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    Exception ex = (Exception)args.ExceptionObject;
    // Log the exception details
    File.Create("Errtest");
    Console.WriteLine(ex.ToString());
    { }
};
// Act
throw new Exception("Test exception");