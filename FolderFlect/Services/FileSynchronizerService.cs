using FolderFlect.Config;
using NLog;
using FolderFlect.Models;
using FolderFlect.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using FolderFlect.Services;
using FolderFlect.Services.IServices;
using FolderFlect.Extensions;

public class FileSynchronizerService : IFileSynchronizerService
{
    #region Fields and Constructor

    private readonly ILogger _logger;
    private readonly string _sourcePath;
    private readonly string _replicaPath;
    private readonly IFileProcessorService _fileProcessor;

    public FileSynchronizerService(ILogger logger, AppConfig appConfig, IFileProcessorService fileProcessorService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sourcePath = !string.IsNullOrWhiteSpace(appConfig.SourcePath) ? appConfig.SourcePath : throw new ArgumentException("SourcePath cannot be null or empty.", nameof(appConfig.SourcePath));
        _replicaPath = !string.IsNullOrWhiteSpace(appConfig.ReplicaPath) ? appConfig.ReplicaPath : throw new ArgumentException("ReplicaPath cannot be null or empty.", nameof(appConfig.ReplicaPath));
        _fileProcessor = fileProcessorService ?? throw new ArgumentNullException(nameof(fileProcessorService));
        _logger.Debug("FileSynchronizerService initialized with source and replica paths.");

    }

    #endregion

    public Result SyncFilesByMD5(FilesToSyncSetByMD5 filesToSyncSet)
    {
        _logger.Debug("Starting SyncFilesByMD5...");

        List<OperationFileProcessorResult> results = new List<OperationFileProcessorResult>();

        try
        {
            results.Add(CreateDirectories(filesToSyncSet.DirectoriesToCreate));
            results.Add(MoveFiles(filesToSyncSet.FilesToMove));
            results.Add(CopyFilesToDestination(filesToSyncSet.FilesToCopy));
            results.Add(DeleteFilesFromDestination(filesToSyncSet.FilesToDelete));
            results.Add(DeleteDirectories(filesToSyncSet.DirectoriesToDelete));

            _logger.LogSyncResult(results);

            if (AnyFailures(results))
            {
                _logger.Warn("File synchronization completed with issues.");
                return Result.Success();
            }

            _logger.Debug("Successfully synced files by MD5.");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred during file synchronization.");
            return Result.Fail($"Error occurred during file synchronization: {ex.Message}");
        }
    }

    private OperationFileProcessorResult DeleteFilesFromDestination(IEnumerable<string> pathsToDelete)
    {
        _logger.Debug("Starting deletion of files from destination...");
        var deleteResult = _fileProcessor.DeleteFiles(GetAbsolutePaths(pathsToDelete, _replicaPath));
        return new OperationFileProcessorResult(deleteResult, "File Deletion");
    }

    private OperationFileProcessorResult CopyFilesToDestination(IEnumerable<string> pathsToCopy)
    {
        _logger.Debug("Starting copying files to destination...");
        var absolutePathsToCopy = new List<(string, string)>();
        foreach (var relativePath in pathsToCopy)
        {
            var sourceFilePath = Path.Combine(_sourcePath, relativePath);
            var destinationFilePath = Path.Combine(_replicaPath, relativePath);
            absolutePathsToCopy.Add((sourceFilePath, destinationFilePath));
        }
        var copyResult = _fileProcessor.CopyFiles(absolutePathsToCopy);
        return new OperationFileProcessorResult(copyResult, "File Copying");
    }

    private OperationFileProcessorResult DeleteDirectories(List<string> directoriesToDelete)
    {
        _logger.Debug("Starting deleting directories...");
        var deleteDirResult = _fileProcessor.DeleteDirectories(GetAbsolutePaths(directoriesToDelete, _replicaPath));
        return new OperationFileProcessorResult(deleteDirResult, "Directory Deletion");
    }

    private OperationFileProcessorResult CreateDirectories(List<string> directoriesToCreate)
    {
        _logger.Debug("Starting creating directories...");
        var createDirResult = _fileProcessor.CreateDirectories(GetAbsolutePaths(directoriesToCreate, _replicaPath));
        return new OperationFileProcessorResult(createDirResult, "Directory Creation");
    }

    private OperationFileProcessorResult MoveFiles(List<(string, string)> filesToMove)
    {
        _logger.Debug("Starting moving files...");
        var absolutePathsToMove = new List<(string, string)>();
        foreach (var (initialPath, finalPath) in filesToMove)
        {
            var sourceFilePath = Path.Combine(_replicaPath, initialPath);
            var destinationFilePath = Path.Combine(_replicaPath, finalPath);
            absolutePathsToMove.Add((sourceFilePath, destinationFilePath));
        }
        var moveResult = _fileProcessor.MoveFiles(absolutePathsToMove);
        return new OperationFileProcessorResult(moveResult, "File Moving");
    }

    private List<string> GetAbsolutePaths(IEnumerable<string> relativePaths, string basePath)
    {
        var absolutePaths = new List<string>();
        foreach (var relativePath in relativePaths)
        {
            absolutePaths.Add(Path.Combine(basePath, relativePath));
        }
        return absolutePaths;
    }

    private bool AnyFailures(List<OperationFileProcessorResult> results)
    {
        return results.Any(r => r.HasFailures());
    }
}
