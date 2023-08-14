using PowerArgs;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using static FolderFlect.Constants.ArgumentKeys;

namespace FolderFlect.Config
{
    [ArgExample("FolderFlect -source C:\\FolderFlect\\source -replica C:\\FolderFlect\\replica -interval 60 -log log.txt", "An example of synchronization every minute")]
    public record CommandLineConfig
    {
        [ArgShortcut(Source), ArgDescription("Source path"), ArgExistingDirectory, ArgRequired]
        public string SourcePath { get; init; }

        [ArgShortcut(Replica), ArgDescription("Replica path"), ArgRequired]
        public string ReplicaPath { get; init; }

        [ArgShortcut(Interval), ArgDescription("Interval in seconds"), ArgRequired]
        public int SyncInterval { get; init; }

        [ArgShortcut(Log), ArgDescription("Log file path"), ArgRequired]
        public string LogFilePath { get; init; }
        
        [ArgIgnore]
        public (string Path, string Name) SourcePathInfo =>
            (SourcePath, GetDescriptionFromAttribute(typeof(CommandLineConfig), nameof(SourcePath)));

        [ArgIgnore]
        public (string Path, string Name) ReplicaPathInfo =>
            (ReplicaPath, GetDescriptionFromAttribute(typeof(CommandLineConfig), nameof(ReplicaPath)));

        private static string GetDescriptionFromAttribute(Type type, string propertyName)
        {
            var property = type.GetProperty(propertyName);
            var attribute = property?.GetCustomAttribute<ArgDescription>();
            return attribute?.Description;
        }

        public override string ToString()
        {
            var maxDescriptionLength = GetType().GetProperties()
                .Select(prop => prop.GetCustomAttribute<ArgDescription>())
                .Where(attr => attr != null)
                .Max(attr => attr.Description.Length);

            var labelColumnWidth = maxDescriptionLength + 1;

            var output = new StringBuilder();
            output.AppendLine();

            foreach (var prop in GetType().GetProperties())
            {
                var attribute = prop.GetCustomAttribute<ArgDescription>();
                if (attribute != null)
                {
                    var label = $"{attribute.Description}:".PadRight(labelColumnWidth, ' ');
                    var value = prop.GetValue(this)?.ToString() ?? string.Empty;
                    output.AppendLine($"{label} {value}");
                }
            }

            return output.ToString();
        }
    }
}
