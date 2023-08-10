using FolderFlect.Config;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderFlect.Helpers
{
    public static class NLogConfiguraion
    {
        public static void ConfigureNLog(AppConfig appConfig)
        {
            var logFilePath = appConfig.LogFilePath;
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: Console and File
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = logFilePath,
                Layout = "${longdate} ${uppercase:${level}} ${message}"
            };

            var logconsole = new NLog.Targets.ConsoleTarget("logconsole")
            {
                Layout = "${longdate} ${uppercase:${level}} ${message}"
            };

            // Rules for mapping loggers to targets
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config
            NLog.LogManager.Configuration = config;
        }

    }
}
