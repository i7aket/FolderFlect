using FolderFlect.Logging;
using System;

public class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        WriteToConsole($"INFO: {message}", ConsoleColor.White);
    }

    public void Error(string message)
    {
        WriteToConsole($"ERROR: {message}", ConsoleColor.Red);
    }

    private void WriteToConsole(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine($"{DateTime.Now}: {message}");
        Console.ResetColor();  // Reset to default color
    }
}
