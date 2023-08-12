using FolderFlect.Config;
using FolderFlect.Models;
using FolderFlect.Services.IServices;
using FolderFlect.Utilities;
using NLog;

namespace FolderFlect.Services
{
    public class SynchronisationManagerService : ISynchronisationManagerService
    {
        private readonly AppConfig _config;
        private readonly ILogger _logger;
        private readonly IFileSynchronizerService _fileSynchronizerService;
        private readonly IFileScannerService _filesScannerService;
        private readonly IFileComparerService _fileComparerService;
        private readonly ISchedulerService _scheduler;

        public SynchronisationManagerService(
            AppConfig config,
            ILogger logger,
            IFileSynchronizerService fileSynchronizerService,
            IFileScannerService filesScannerService,
            IFileComparerService fileComparerService,
            ISchedulerService scheduler)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            Helpers.NLogConfiguraion.ConfigureNLog(_config);

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileSynchronizerService = fileSynchronizerService ?? throw new ArgumentNullException(nameof(fileSynchronizerService));
            _filesScannerService = filesScannerService ?? throw new ArgumentNullException(nameof(filesScannerService));
            _fileComparerService = fileComparerService ?? throw new ArgumentNullException(nameof(fileComparerService));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));

            _scheduler.OnExecute += PerformSynchronisation;

            _logger.Debug("SynchronisationManagerService constructed successfully.");

        }

        private void PerformSynchronisation()
        {

            _logger.Debug("Starting PerformSynchronisation...");

            var RetrieveFilesGroupedByMD5AndDirectoryPathsResult = _filesScannerService.RetrieveFilesGroupedByMD5AndDirectoryPaths();
            if (!RetrieveFilesGroupedByMD5AndDirectoryPathsResult.IsSuccess)
            {
                _logger.Error(RetrieveFilesGroupedByMD5AndDirectoryPathsResult.Message);
                _logger.Debug("PerformSynchronisation terminated early due to RetrieveFilesGroupedByMD5AndDirectoryPaths failure.");
                return;
            }

            var filesGroupedByMD5AndDirectoryPaths = RetrieveFilesGroupedByMD5AndDirectoryPathsResult.Value;

            var GetFilesToSyncGroupedByMD5AndDirectoryPathsResult = _fileComparerService.GetFilesToSyncGroupedByMD5AndDirectoryPaths(filesGroupedByMD5AndDirectoryPaths);
            if (!GetFilesToSyncGroupedByMD5AndDirectoryPathsResult.IsSuccess)
            {
                _logger.Error(GetFilesToSyncGroupedByMD5AndDirectoryPathsResult.Message);
                _logger.Debug("PerformSynchronisation terminated early due to GetFilesToSyncGroupedByMD5AndDirectoryPaths failure.");
                return;
            }

            var FilesToSyncGroupedByMD5AndDirectoryPaths = GetFilesToSyncGroupedByMD5AndDirectoryPathsResult.Value;

            var SyncFileByMD5Result = _fileSynchronizerService.SyncFilesByMD5(FilesToSyncGroupedByMD5AndDirectoryPaths);
            if (!SyncFileByMD5Result.IsSuccess)
            {
                _logger.Error(SyncFileByMD5Result.Message);
                _logger.Debug("PerformSynchronisation terminated early due to SyncFilesByMD5 failure.");
                return;
            }

            _logger.Debug("Finished PerformSynchronisation successfully.");

        }

        public void RunFolderSynchronisation()
        {
            _logger.Debug("Starting folder synchronization...");

            _scheduler.Start();
            _logger.Info(_config + $"\nPress any key to stop...");
            Console.ReadLine();
            _scheduler.Stop();

            _logger.Debug("Folder synchronization stopped.");
        }
    }
}
