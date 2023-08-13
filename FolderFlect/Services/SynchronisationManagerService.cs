using FolderFlect.Config;
using FolderFlect.Models;
using FolderFlect.Services.IServices;
using FolderFlect.Utilities;
using NLog;
using System;

namespace FolderFlect.Services
{
    /// <summary>
    /// Manages the synchronization of files, utilizing other services to scan, compare, and synchronize.
    /// </summary>
    public class SynchronisationManagerService : ISynchronisationManagerService
    {
        #region Fields and Constructor

        private readonly CommandLineConfig _config;
        private readonly ILogger _logger;
        private readonly IFileSynchronizerService _syncService;
        private readonly IFileScannerService _scannerService;
        private readonly IFileComparerService _comparerService;
        private readonly ISchedulerService _scheduler;


        public SynchronisationManagerService(
            CommandLineConfig config,
            ILogger logger,
            IFileSynchronizerService syncService,
            IFileScannerService scannerService,
            IFileComparerService comparerService,
            ISchedulerService scheduler)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            Helpers.NLogConfiguraion.ConfigureNLog(_config); // Set up NLog configurations.

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
            _scannerService = scannerService ?? throw new ArgumentNullException(nameof(scannerService));
            _comparerService = comparerService ?? throw new ArgumentNullException(nameof(comparerService));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

            _scheduler.OnExecute += Sync; // Subscribe to sync tasks from the scheduler.

            _logger.Debug("Service initialized.");
        }

        #endregion

        /// <summary>
        /// Initiates the synchronization process.
        /// </summary>
        public void StartSync()
        {
            _logger.Debug("Starting synchronization...");

            _scheduler.Start(); 
            _logger.Info($"{_config}\nPress any key to stop...");
            Console.ReadLine(); 
            _scheduler.Stop(); 

            _logger.Debug("Synchronization stopped.");
        }

        /// <summary>
        /// Executes the full synchronization process: scanning, comparing, and syncing files.
        /// </summary>
        private void Sync()
        {
            _logger.Debug("Starting synchronization...");

            var scanResult = _scannerService.RetrieveFilesGroupedByMD5AndDirectoryPaths();
            ProcessResult(scanResult, "Scanning Files");

            var filesToSyncResult = _comparerService.GetFilesToSyncGroupedByMD5AndDirectoryPaths(scanResult.Value);
            ProcessResult(filesToSyncResult, "Comparing Files");

            var syncResult = _syncService.SyncFilesByMD5(filesToSyncResult.Value);
            ProcessResult(syncResult, "Syncing Files");

            _logger.Debug("Synchronization finished.");
        }

        /// <summary>
        /// Processes the result of an operation, logging any errors encountered.
        /// </summary>
        /// <param name="result">The result of the operation.</param>
        /// <param name="operationName">The name of the operation for logging purposes.</param>
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
