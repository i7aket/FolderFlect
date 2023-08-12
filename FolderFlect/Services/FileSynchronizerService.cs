using FolderFlect.Config;
using NLog;
using FolderFlect.Models;
using FolderFlect.Utilities;

public class FileSynchronizerService : IFileSynchronizerService
{
    private readonly ILogger _logger;
    private readonly string _sourcePath;
    private readonly string _replicaPath;

    public FileSynchronizerService(ILogger logger, AppConfig appConfig)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sourcePath = appConfig.SourcePath;
        _replicaPath = appConfig.ReplicaPath;
        _logger.Debug("FileSynchronizerService initialized with source and replica paths.");

    }

    public Result SyncFilesByMD5(FilesToSyncSetByMD5 filesToSyncSet)
    {
        _logger.Debug("Starting SyncFilesByMD5...");

        try
        {
            CreateDirectories(filesToSyncSet.DirectoriesToCreate);
            MoveFiles(filesToSyncSet.FilesToMove);
            CopyFilesToDestination(filesToSyncSet.FilesToCopy);
            DeleteFilesFromDestination(filesToSyncSet.FilesToDelete);
            DeleteDirectories(filesToSyncSet.DirectoriesToDelete);
            _logger.Debug("Successfully synced files by MD5.");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred during file synchronization.");
            return Result.Fail("Error occurred during file synchronization: " + ex.Message);
        }
    }
    private void DeleteFilesFromDestination(IEnumerable<string> pathsToDelete)
    {
        _logger.Debug("Starting deletion of files from destination...");

        foreach (var relativePath in pathsToDelete)
        {
            try
            {
                var fullPathToDelete = Path.Combine(_replicaPath, relativePath);
                if (File.Exists(fullPathToDelete))
                {
                    var file = new FileInfo(fullPathToDelete);
                    SetFileAsWritable(file);
                    file.Delete();
                    _logger.Info($"File {fullPathToDelete} successfully deleted.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error deleting file {relativePath}.");
            }
        }
        _logger.Debug("Completed deletion of files from destination.");

    }

    private void CopyFilesToDestination(IEnumerable<string> pathsToCopy)
    {
        _logger.Debug("Starting copying files to destination...");

        foreach (var relativePath in pathsToCopy)
        {
            try
            {
                var sourceFilePath = Path.Combine(_sourcePath, relativePath);
                var destinationFilePath = Path.Combine(_replicaPath, relativePath);

                if (File.Exists(destinationFilePath))
                {
                    var destFile = new FileInfo(destinationFilePath);
                    SetFileAsWritable(destFile);
                }

                if (File.Exists(sourceFilePath))
                {
                    var destinationFolder = Path.GetDirectoryName(destinationFilePath);
                    Directory.CreateDirectory(destinationFolder);

                    using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var destStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        sourceStream.CopyTo(destStream);
                    }

                    File.SetAttributes(destinationFilePath, File.GetAttributes(sourceFilePath));
                    _logger.Info($"File {sourceFilePath} successfully copied to {destinationFilePath}.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error copying file {relativePath}.");
            }
        }
        _logger.Debug("Completed copying files to destination.");

    }

    private void DeleteDirectories(List<string> directoriesToDelete)
    {
        _logger.Debug("Starting deleting directories...");

        foreach (var relativeDirectoryPath in directoriesToDelete)
        {
            try
            {
                var fullPathToDelete = Path.Combine(_replicaPath, relativeDirectoryPath);

                if (Directory.Exists(fullPathToDelete))
                {
                    Directory.Delete(fullPathToDelete, recursive: true);
                    _logger.Info($"Deleted directory: {fullPathToDelete}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error deleting directory {relativeDirectoryPath}.");
            }
        }
        _logger.Debug("Completed deleting directories.");

    }

    private void CreateDirectories(List<string> directoriesToCreate)
    {
        _logger.Debug("Starting creating directories...");

        foreach (var relativeDirectoryPath in directoriesToCreate)
        {
            try
            {
                var fullPathToCreate = Path.Combine(_replicaPath, relativeDirectoryPath);

                if (!Directory.Exists(fullPathToCreate))
                {
                    Directory.CreateDirectory(fullPathToCreate);
                    _logger.Info($"Created directory: {fullPathToCreate}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error creating directory {relativeDirectoryPath}.");
            }
        }
        _logger.Debug("Completed creating directories.");

    }

    public void MoveFiles(List<(string, string)> filesToMove)
    {
        _logger.Debug("Starting moving files...");

        foreach (var (initialPath, finalPath) in filesToMove)
        {
            try
            {
                var sourceFilePath = Path.Combine(_replicaPath, initialPath);
                var destinationFilePath = Path.Combine(_replicaPath, finalPath);

                if (File.Exists(sourceFilePath))
                {
                    var destinationFolder = Path.GetDirectoryName(destinationFilePath);
                    Directory.CreateDirectory(destinationFolder);

                    File.Move(sourceFilePath, destinationFilePath, overwrite: true);
                    _logger.Info($"File {sourceFilePath} successfully moved to {destinationFilePath}.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error moving file from {initialPath} to {finalPath}.");
            }
        }
        _logger.Debug("Completed moving files.");

    }

    private void SetFileAsWritable(FileInfo file)
    {
        if (file.IsReadOnly)
            file.IsReadOnly = false;
    }



}

