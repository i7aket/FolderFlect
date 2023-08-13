using FolderFlect.Config;
using FolderFlect.Constants;
using FolderFlect.Utilities;

namespace FolderFlect.Helpers
{
    /// <summary>
    /// Provides functionality to load application configurations from command-line arguments.
    /// </summary>
    public static class ConfigurationLoader
    {
        public static Result<AppConfig> LoadConfiguration(string[] args)
        {
            var validation = ValidateArgumentsPresent(args);
            if (!validation.IsSuccess)
            {
                return Result<AppConfig>.Fail(validation.Message);
            }

            var parseResult = ParseArguments(args);
            if (!parseResult.IsSuccess)
            {
                return Result<AppConfig>.Fail(parseResult.Message);
            }

            var config = new AppConfig(parseResult.Value.sourcePath, parseResult.Value.replicaPath, parseResult.Value.syncInterval, parseResult.Value.logFilePath);
            return Result<AppConfig>.Success(config);
        }

        private static Result ValidateArgumentsPresent(string[] args)
        {
            if (args.Length == 0)
            {
                return Result.Fail("Error: No arguments provided.\n" +
                                   "Usage example: -source [source path] -replica [replica path] -interval [interval in seconds] -log [log file path]");
            }

            return Result.Success();
        }

        private static Result<(string sourcePath, string replicaPath, int syncInterval, string logFilePath)> ParseArguments(string[] args)
        {
            string sourcePath = string.Empty;
            string replicaPath = string.Empty;
            int syncInterval = 0;
            string logFilePath = string.Empty;

            for (int i = 0; i < args.Length; i++)
            {
                string nextArg = (i + 1) < args.Length ? args[i + 1] : null;

                if (string.IsNullOrEmpty(nextArg) || nextArg.StartsWith("-"))
                {
                    return Result<(string, string, int, string)>.Fail($"Error: Expected a value after '{args[i]}'.");
                }

                switch (args[i])
                {
                    case ArgumentKeys.Source:
                        sourcePath = args[++i];
                        break;

                    case ArgumentKeys.Replica:
                        replicaPath = args[++i];
                        break;

                    case ArgumentKeys.Interval:
                        if (!int.TryParse(args[++i], out syncInterval) || syncInterval <= 0)
                        {
                            return Result<(string, string, int, string)>.Fail($"Error: Interval must be a positive number. Received: {args[i]}.");
                        }
                        break;

                    case ArgumentKeys.Log:
                        logFilePath = args[++i];
                        break;

                    default:
                        return Result<(string, string, int, string)>.Fail($"Error: Unknown argument '{args[i]}'.");
                }
            }

            var validation = AreConfigurationValuesValid(sourcePath, replicaPath, syncInterval, logFilePath);
            if (!validation.IsSuccess)
            {
                return Result<(string, string, int, string)>.Fail(validation.Message);
            }
            return Result<(string, string, int, string)>.Success((sourcePath, replicaPath, syncInterval, logFilePath));
        }

        private static Result AreConfigurationValuesValid(string sourcePath, string replicaPath, int syncInterval, string logFilePath)
        {
            if (string.IsNullOrEmpty(sourcePath))
            {
                return Result.Fail("Error: Source path is not provided.");
            }

            if (string.IsNullOrEmpty(replicaPath))
            {
                return Result.Fail("Error: Replica path is not provided.");
            }

            if (syncInterval <= 0)
            {
                return Result.Fail("Error: Sync interval must be a positive number.");
            }

            if (string.IsNullOrEmpty(logFilePath))
            {
                return Result.Fail("Error: Log file path is not provided.");
            }

            return Result.Success();
        }
    }
}
