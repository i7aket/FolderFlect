using FolderFlect.Config;
using FolderFlect.Models;
using FolderFlect.Services.IServices;
using FolderFlect.Utilities;
using NLog;
using System;

namespace FolderFlect.Services
{
    public class SynchronisationManagerService : ISynchronisationManagerService
    {
        #region Fields and Constructor

        private readonly AppConfig _config;
        private readonly ILogger _logger;
        private readonly IFileSynchronizerService _syncService;
        private readonly IFileScannerService _scannerService;
        private readonly IFileComparerService _comparerService;
        private readonly ISchedulerService _scheduler;

        public SynchronisationManagerService(
            AppConfig config,
            ILogger logger,
            IFileSynchronizerService syncService,
            IFileScannerService scannerService,
            IFileComparerService comparerService,
            ISchedulerService scheduler)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            Helpers.NLogConfiguraion.ConfigureNLog(_config);

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
            _scannerService = scannerService ?? throw new ArgumentNullException(nameof(scannerService));
            _comparerService = comparerService ?? throw new ArgumentNullException(nameof(comparerService));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

            _scheduler.OnExecute += Sync;

            _logger.Debug("Service initialized.");
        }

        #endregion

        public void StartSync()
        {
            _logger.Debug("Starting synchronization...");

            _scheduler.Start();
            _logger.Info($"{_config}\nPress any key to stop...");
            Console.ReadLine();
            _scheduler.Stop();

            _logger.Debug("Synchronization stopped.");
        }

        private void Sync()
        {
            _logger.Debug("Starting synchronization...");

            var scanResult = _scannerService.RetrieveFilesGroupedByMD5AndDirectoryPaths();
            ProcessResult(scanResult, "Scanning Files");

            var filesToSync = _comparerService.GetFilesToSyncGroupedByMD5AndDirectoryPaths(scanResult.Value);
            ProcessResult(filesToSync, "Comparing Files");

            var syncResult = _syncService.SyncFilesByMD5(filesToSync.Value);
            ProcessResult(syncResult, "Syncing Files");

            _logger.Debug("Synchronization finished.");
        }

        private void ProcessResult(IResult result, string operationName)
        {
            if (!result.IsSuccess)
            {
                _logger.Error(result.Message);
                _logger.Debug($"PerformSynchronisation terminated early due to {operationName} failure.");
                return;
            }
        }

    }
}
