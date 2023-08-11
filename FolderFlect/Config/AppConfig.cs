using FolderFlect.Constants;
using System;

namespace FolderFlect.Config
{
    public record AppConfig(
        string SourcePath,
        string ReplicaPath,
        int SyncInterval,
        string LogFilePath)
    {
        public override string ToString()
        {
            var labelColumnWidth = 26;

            return $"\n" +
                   $"{GetPaddedString("Source Folder:", labelColumnWidth)} {SourcePath}\n" +
                   $"{GetPaddedString("Destination Folder:", labelColumnWidth)} {ReplicaPath}\n" +
                   $"{GetPaddedString("Synchronization Frequency:", labelColumnWidth)} {GetReadableTimeInterval(SyncInterval)}\n" +
                   $"{GetPaddedString("Log File Location:", labelColumnWidth)} {LogFilePath}";
        }

        private string GetPaddedString(string label, int totalWidth)
        {
            return label.PadRight(totalWidth, ' ');
        }

        private string GetReadableTimeInterval(int seconds)
        {
            int days = seconds / 86400;
            int remainder = seconds % 86400;
            int hours = remainder / 3600;
            remainder %= 3600;
            int minutes = remainder / 60;
            seconds = remainder % 60;

            string result = "";

            if (days > 0)
            {
                result += $"{days}d ";
            }
            if (hours > 0 || days > 0)
            {
                result += $"{hours}h ";
            }
            if (minutes > 0 || hours > 0 || days > 0)
            {
                result += $"{minutes}m ";
            }
            result += $"{seconds}s";

            return result.Trim();
        }
    }
}
