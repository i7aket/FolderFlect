using FolderFlect.Config;
using FolderFlect.Logging;
using FolderFlect.Models;
using FolderFlect.Services.IServices;
using FolderFlect.Utilities;

namespace FolderFlect.Services
{
    public class SynchronisationManagerService 
    {
        private readonly AppConfig _config;
        private readonly ILogger _logger;
        private readonly IDirectorySynchronizerService _directorySynchronizerService;
        private readonly IFileSynchronizerService _fileSynchronizerService;
        private readonly IFileScannerService _filesScannerService;
        private readonly IFileComparerService _fileComparerService;
        private readonly IDirectoryScannerService _directoryScannerService;
        private readonly IDirectoryComparerService _directoryComparerService;
        private readonly ISchedulerService _scheduler;

        public SynchronisationManagerService(AppConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = new FileAndConsoleLogger(_config.LogFilePath);
            _directorySynchronizerService = new DirectorySynchronizerService(_logger, _config);
            _fileSynchronizerService = new FileSynchronizerService(_logger, _config);
            _filesScannerService = new FileScannerService(_logger, _config);
            _fileComparerService = new FileComparerService(_logger);
            _directoryScannerService = new DirectoryScannerService(_logger, _config);
            _directoryComparerService = new DirectoryComparerService(_logger);
            _scheduler = new SchedulerService(_logger, _config);
        }

        public void RunFolderSynchronisation()
        {
            _logger.Log("Synchronisation Service has started.\n" +
                "Scheduler service has been configured.");

            _scheduler.OnExecute += () =>
            {
                var sourceFilesResult = _filesScannerService.ScanSourceDirectoryByRelativePath();
                if (!sourceFilesResult.IsSuccess)
                {
                    _logger.Log($"Error scanning source directory: {sourceFilesResult.Message}");
                    return;
                }

                var destFilesResult = _filesScannerService.ScanDestinationDirectoryByRelativePath();
                if (!destFilesResult.IsSuccess)
                {
                    _logger.Log($"Error scanning destination directory: {destFilesResult.Message}");
                    return;
                }

                var filesToDeleteResult = _fileComparerService.GetFilesToDeleteFromDestination(sourceFilesResult.Value, destFilesResult.Value);
                if (!filesToDeleteResult.IsSuccess)
                {
                    _logger.Log($"Error comparing files for deletion: {filesToDeleteResult.Message}");
                    return;
                }

                var deleteFilesResult = _fileSynchronizerService.DeleteFilesFromDestination(filesToDeleteResult.Value);
                if (!deleteFilesResult.IsSuccess)
                {
                    _logger.Log($"Error deleting files: {deleteFilesResult.Message}");
                    return;
                }

                var sourceDirectoriesResult = _directoryScannerService.ScanSourceDirectoryByRelativePath();
                if (!sourceDirectoriesResult.IsSuccess)
                {
                    _logger.Log($"Error scanning source directories: {sourceDirectoriesResult.Message}");
                    return;
                }

                var destDirectoriesResult = _directoryScannerService.ScanDestinationDirectoryByRelativePath();
                if (!destDirectoriesResult.IsSuccess)
                {
                    _logger.Log($"Error scanning destination directories: {destDirectoriesResult.Message}");
                    return;
                }

                var dirToDeleteResult = _directoryComparerService.GetDirectoriesToDelete(sourceDirectoriesResult.Value, destDirectoriesResult.Value);
                if (!dirToDeleteResult.IsSuccess)
                {
                    _logger.Log($"Error comparing directories for deletion: {dirToDeleteResult.Message}");
                    return;
                }

                var deleteDirectoriesResult = _directorySynchronizerService.DeleteDirectories(dirToDeleteResult.Value);
                if (!deleteDirectoriesResult.IsSuccess)
                {
                    _logger.Log($"Error deleting directories: {deleteDirectoriesResult.Message}");
                    return;
                }

                var dirToCreateResult = _directoryComparerService.GetDirectoriesToCreate(sourceDirectoriesResult.Value, destDirectoriesResult.Value);
                if (!dirToCreateResult.IsSuccess)
                {
                    _logger.Log($"Error comparing directories for creation: {dirToCreateResult.Message}");
                    return;
                }

                var createDirectoriesResult = _directorySynchronizerService.CreateDirectories(dirToCreateResult.Value);
                if (!createDirectoriesResult.IsSuccess)
                {
                    _logger.Log($"Error creating directories: {createDirectoriesResult.Message}");
                    return;
                }

                var filesToCopyResult = _fileComparerService.GetFilesToCopy(sourceFilesResult.Value, destFilesResult.Value);
                if (!filesToCopyResult.IsSuccess)
                {
                    _logger.Log($"Error comparing files for copying: {filesToCopyResult.Message}");
                    return;
                }

                var copyFilesResult = _fileSynchronizerService.CopyFilesToDestination(filesToCopyResult.Value);
                if (!copyFilesResult.IsSuccess)
                {
                    _logger.Log($"Error copying files: {copyFilesResult.Message}");
                    return;
                }

                var intersectingFilesResult = _fileComparerService.GetIntersectingFiles(sourceFilesResult.Value, destFilesResult.Value);
                if (!intersectingFilesResult.IsSuccess)
                {
                    _logger.Log($"Error finding intersecting files: {intersectingFilesResult.Message}");
                    return;
                }

                var filesToUpdateResult = _fileComparerService.GetFilesForUpdate(intersectingFilesResult.Value, sourceFilesResult.Value, destFilesResult.Value);
                if (!filesToUpdateResult.IsSuccess)
                {
                    _logger.Log($"Error comparing files for updates: {filesToUpdateResult.Message}");
                    return;
                }

                var updateFilesResult = _fileSynchronizerService.CopyFilesToDestination(filesToUpdateResult.Value);
                if (!updateFilesResult.IsSuccess)
                {
                    _logger.Log($"Error updating files: {updateFilesResult.Message}");
                    return;
                }

                var filesForAttributesUpdate = _fileComparerService.GetFilesForAttributesUpdate(intersectingFilesResult.Value, sourceFilesResult.Value, destFilesResult.Value);
                if (!filesForAttributesUpdate.IsSuccess)
                {
                    _logger.Log($"Error comparing files for attribute updates: {filesForAttributesUpdate.Message}");
                    return;
                }

                var AttributesUpdateResult = _fileSynchronizerService.UpdateFileAttributes(filesForAttributesUpdate.Value);
                if (!AttributesUpdateResult.IsSuccess)
                {
                    _logger.Log($"Error updating file attributes: {AttributesUpdateResult.Message}");
                    return;
                }
            };



            _scheduler.Start();
            _logger.Log("Schedule Service has started.");

            Console.WriteLine($"Schedule Service has started." +
                $"\nSynchronization will be performed every {_config.SyncInterval} seconds." +
                $"\nPress any key to stop...");

            Console.ReadLine();
            _scheduler.Stop();
        }
    }
}