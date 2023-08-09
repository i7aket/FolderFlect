using FolderFlect.Logging;
using System;
using System.IO;

public class FileLogger : ILogger
{
    private readonly string _filePath;

    public FileLogger(string filePath)
    {
        _filePath = filePath;
    }

    public void Log(string message)
    {
        WriteToFile($"INFO: {message}");
    }

    public void Error(string message)
    {
        WriteToFile($"ERROR: {message}");
    }

    private void WriteToFile(string message)
    {
        using (StreamWriter sw = new StreamWriter(_filePath, true))
        {
            sw.WriteLine($"{DateTime.Now}: {message}");
        }
    }
}
