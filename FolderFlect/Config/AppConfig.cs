using FolderFlect.Constants;
using FolderFlect.Utilities;
using System;

namespace FolderFlect.Config
{
    public record AppConfig(
        string SourcePath,
        string ReplicaPath,
        int SyncInterval,
        string LogFilePath)
    {
        public (string Path, string Name) SourcePathInfo => (SourcePath, "Source Path");
        public (string Path, string Name) ReplicaPathInfo => (ReplicaPath, "Replica Path");

        public override string ToString()
        {
            var labelColumnWidth = 26;

            return $"\n" +
                   $"{GetPaddedString("Source Folder:", labelColumnWidth)} {SourcePath}\n" +
                   $"{GetPaddedString("Destination Folder:", labelColumnWidth)} {ReplicaPath}\n" +
                   $"{GetPaddedString("Synchronization Frequency:", labelColumnWidth)} {TimeHelper.GetInterval(SyncInterval)}\n" +
                   $"{GetPaddedString("Log File Location:", labelColumnWidth)} {LogFilePath}";
        }

        private string GetPaddedString(string label, int totalWidth)
        {
            return label.PadRight(totalWidth, ' ');
        }
    }
}
