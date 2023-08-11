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
        }

        private void PerformSynchronisation()
        {
            var directoryFileSetResult = _filesScannerService.GetAllRelativeFilePaths();
            if (!directoryFileSetResult.IsSuccess)
            {
                _logger.Error(directoryFileSetResult.Message);
                return;
            }

            var filesToSyncSetResult = _fileComparerService.GetFilesToSync(directoryFileSetResult.Value);
            if (!filesToSyncSetResult.IsSuccess)
            {
                _logger.Error(filesToSyncSetResult.Message);
                return;
            }


            var syncFilesResult = _fileSynchronizerService.SyncFiles(filesToSyncSetResult.Value);
            if (!syncFilesResult.IsSuccess)
            {
                _logger.Error(syncFilesResult.Message);
                return;
            }

        }

        public void RunFolderSynchronisation()
        {
            _scheduler.Start();
            _logger.Info(_config + $"\nPress any key to stop...");
            Console.ReadLine();
            _scheduler.Stop();
        }
    }
}
