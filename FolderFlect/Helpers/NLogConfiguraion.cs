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
        public static void ConfigureNLog(CommandLineConfig appConfig)
        {
            var logFilePath = appConfig.LogFilePath;
            var debugLogFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(logFilePath), "debuglog.txt");

            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: Console, Main log file, and Debug log file
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = logFilePath,
                Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} ${uppercase:${level}} ${message}"
            };

            var debugfile = new NLog.Targets.FileTarget("debugfile")
            {
                FileName = debugLogFilePath,
                Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} ${uppercase:${level}} ${message}"
            };

            var logconsole = new NLog.Targets.ConsoleTarget("logconsole")
            {
                Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} ${uppercase:${level}} ${message}"
            };

            // Rules for mapping loggers to targets
            config.AddRule(LogLevel.Info, LogLevel.Info, logconsole); // Info to Console
            config.AddRule(LogLevel.Info, LogLevel.Info, logfile);    // Info to Main log file

            config.AddRule(LogLevel.Debug, LogLevel.Debug, debugfile); // Debug to Debug log file
            config.AddRule(LogLevel.Error, LogLevel.Error, debugfile); // Error to Debug log file
            config.AddRule(LogLevel.Warn, LogLevel.Warn, debugfile); // Warning to Debug log file


            // Apply config
            NLog.LogManager.Configuration = config;

        }

    }
}
