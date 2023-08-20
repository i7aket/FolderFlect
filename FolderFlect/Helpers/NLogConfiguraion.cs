using FolderFlect.Config;
using NLog;

namespace FolderFlect.Helpers
{
    public static class NLogConfiguraion
    {
        public static void ConfigureNLog(CommandLineConfig appConfig)
        {
            var logFilePath = appConfig.LogFilePath;

            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: Console and Main log file
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = logFilePath,
                Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} ${uppercase:${level}} ${message}"
            };

            var logconsole = new NLog.Targets.ConsoleTarget("logconsole")
            {
                Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} ${uppercase:${level}} ${message}"
            };

            if (DebugMode.IsEnabled)
            {
                config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole); 
                config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile); 
            }
            else
            {
                config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
                config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            }
            // Apply config
            NLog.LogManager.Configuration = config;
        }
    }
}
