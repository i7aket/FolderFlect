using FolderFlect.Config;
using FolderFlect.Constants;
using FolderFlect.Utilities;
using System;
using System.Text;

namespace FolderFlect.Helpers
{
    public static class ConfigurationLoader
    {
        public static Result<AppConfig> LoadConfiguration(string[] args)
        {
            var message = new StringBuilder();

            string sourcePath = string.Empty;
            string replicaPath = string.Empty;
            int syncInterval = 0;
            string logFilePath = string.Empty;

            bool hasError = args.Length == 0;

            for (int i = 0; i < args.Length; i++)
            {
                string nextArg = (i + 1) < args.Length ? args[i + 1] : null;

                if (string.IsNullOrEmpty(nextArg) || nextArg.StartsWith("-"))
                {
                    message.AppendLine($"Error: Expected a value after '{args[i]}'.");
                    hasError = true;
                    continue;
                }

                switch (args[i])
                {
                    case ArgumentKeys.Source:
                        sourcePath = args[++i];
                        if (string.IsNullOrEmpty(sourcePath))
                        {
                            message.AppendLine($"SourcePath - Error");
                            hasError = true;
                            break;
                        }
                        message.AppendLine($"SourcePath - {sourcePath} - Ok!");
                        break;

                    case ArgumentKeys.Replica:
                        replicaPath = args[++i];
                        if (string.IsNullOrEmpty(replicaPath))
                        {
                            message.AppendLine($"ReplicaPath - Error");
                            hasError = true;
                            break;
                        }
                        message.AppendLine($"ReplicaPath - {replicaPath} - Ok!");
                        break;

                    case ArgumentKeys.Interval:
                        if (!int.TryParse(args[++i], out syncInterval))
                        {
                            message.AppendLine($"Error: Interval must be a number. Received: {args[i]}.");
                            hasError = true;
                            break;
                        }
                        message.AppendLine($"Interval - {syncInterval} - Ok!");
                        break;

                    case ArgumentKeys.Log:
                        logFilePath = args[++i];
                        if (string.IsNullOrEmpty(logFilePath))
                        {
                            message.AppendLine($"LogFilePath - Error");
                            hasError = true;
                            break;
                        }
                        message.AppendLine($"LogFilePath - {logFilePath} - Ok!");
                        break;

                    default:
                        message.AppendLine($"Error: Unknown argument '{args[i]}'.");
                        hasError = true;
                        break;
                }
            }

            if (string.IsNullOrEmpty(sourcePath) ||
                string.IsNullOrEmpty(replicaPath) ||
                syncInterval <= 0 ||
                string.IsNullOrEmpty(logFilePath))
            {
                hasError = true;
            }

            if (hasError)
            {
                message.AppendLine("Error: Not all required arguments were provided.\n" +
                    "Usage example: -source [source path] -replica [replica path] -interval [interval in seconds] -log [log file path]");
                return Result<AppConfig>.Fail(message.ToString());
            }

            var config = new AppConfig(sourcePath, replicaPath, syncInterval, logFilePath);
            return Result<AppConfig>.Success(config);
        }
    }
}
