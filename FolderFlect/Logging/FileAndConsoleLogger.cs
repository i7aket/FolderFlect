using FolderFlect.Logging;
using System;
using System.IO;

public class FileAndConsoleLogger : ILogger
{
    private readonly string _logFilePath;

    public FileAndConsoleLogger(string logFilePath)
    {
        _logFilePath = logFilePath;
    }

    public void Log(string message)
    {
        WriteLog($"INFO: {message}");
        WriteToConsole($"INFO: {message}", ConsoleColor.White);
    }

    public void Error(string message)
    {
        WriteLog($"ERROR: {message}");
        WriteToConsole($"ERROR: {message}", ConsoleColor.Red);
    }

    private void WriteLog(string message)
    {
        string timedMessage = $"{DateTime.Now}: {message}";
        File.AppendAllText(_logFilePath, timedMessage + Environment.NewLine);
    }

    private void WriteToConsole(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine($"{DateTime.Now}: {message}");
        Console.ResetColor();  // Reset to default color
    }
}
