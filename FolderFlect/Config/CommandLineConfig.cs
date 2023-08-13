using System.Reflection;
using System.Text;

namespace FolderFlect.Config
{
    public record CommandLineConfig 
    {
        [CmdOption("source", "Source path")]
        public string SourcePath { get; init; }

        [CmdOption("replica", "Replica path")]
        public string ReplicaPath { get; init; }

        [CmdOption("interval", "Interval in seconds")]
        public int SyncInterval { get; init; }

        [CmdOption("log", "Log file path")]
        public string LogFilePath { get; init; }

        public (string Path, string Name) SourcePathInfo =>
            (SourcePath, CommandLineParser.GetDescriptionFromAttribute(typeof(CommandLineConfig), nameof(SourcePath)));

        public (string Path, string Name) ReplicaPathInfo =>
            (ReplicaPath, CommandLineParser.GetDescriptionFromAttribute(typeof(CommandLineConfig), nameof(ReplicaPath)));

        public override string ToString()
        {
            var maxDescriptionLength = GetType().GetProperties()
                .Select(prop => prop.GetCustomAttribute<CmdOptionAttribute>())
                .Where(attr => attr != null)
                .Max(attr => attr.Description.Length);

            var labelColumnWidth = maxDescriptionLength + 1;

            var output = new StringBuilder();
            output.AppendLine();

            foreach (var prop in GetType().GetProperties())
            {
                var attribute = prop.GetCustomAttribute<CmdOptionAttribute>();
                if (attribute != null)
                {
                    var label = GetPaddedString($"{attribute.Description}:", labelColumnWidth);
                    var value = prop.GetValue(this)?.ToString() ?? string.Empty;
                    output.AppendLine($"{label} {value}");
                }
            }

            return output.ToString();
        }

        protected string GetPaddedString(string label, int totalWidth)
        {
            return label.PadRight(totalWidth, ' ');
        }
    }
}
