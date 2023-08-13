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

/// <summary>
/// Service responsible for synchronizing files between a source path and a replica path based on their MD5 hashes.
/// </summary>
public class FileSynchronizerService : IFileSynchronizerService
{
    #region Fields and Constructor

    private readonly ILogger _logger;
    private readonly string _sourcePath;
    private readonly string _replicaPath;
    private readonly IFileProcessorService _fileProcessor;

    public FileSynchronizerService(ILogger logger, CommandLineConfig appConfig, IFileProcessorService fileProcessorService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sourcePath = !string.IsNullOrWhiteSpace(appConfig.SourcePath) ? appConfig.SourcePath : throw new ArgumentException("SourcePath cannot be null or empty.", nameof(appConfig.SourcePath));
        _replicaPath = !string.IsNullOrWhiteSpace(appConfig.ReplicaPath) ? appConfig.ReplicaPath : throw new ArgumentException("ReplicaPath cannot be null or empty.", nameof(appConfig.ReplicaPath));
        _fileProcessor = fileProcessorService ?? throw new ArgumentNullException(nameof(fileProcessorService));
        _logger.Debug("FileSynchronizerService initialized with source and replica paths.");

    }

    #endregion

    /// <summary>
    /// Synchronizes files by comparing their MD5 hashes.
    /// </summary>
    /// <param name="filesToSyncSet">Set of files to be synchronized.</param>
    /// <returns>The result of the synchronization process.</returns>
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

    /// <summary>
    /// Deletes specified files from the destination.
    /// </summary>
    /// <param name="pathsToDelete">Relative paths of files to delete.</param>
    /// <returns>The result of the delete operation.</returns>
    private OperationFileProcessorResult DeleteFilesFromDestination(IEnumerable<string> pathsToDelete)
    {
        _logger.Debug("Starting deletion of files from destination...");
        var deleteResult = _fileProcessor.DeleteFiles(FileSyncHelper.GetAbsolutePaths(pathsToDelete, _replicaPath));
        return new OperationFileProcessorResult(deleteResult, "File Deletion");
    }

    /// <summary>
    /// Copies specified files to the destination.
    /// </summary>
    /// <param name="pathsToCopy">Relative paths of files to copy.</param>
    /// <returns>The result of the copy operation.</returns>
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

    /// <summary>
    /// Deletes specified directories.
    /// </summary>
    /// <param name="directoriesToDelete">List of directories to delete.</param>
    /// <returns>The result of the delete operation.</returns>
    private OperationFileProcessorResult DeleteDirectories(List<string> directoriesToDelete)
    {
        _logger.Debug("Starting deleting directories...");
        var deleteDirResult = _fileProcessor.DeleteDirectories(FileSyncHelper.GetAbsolutePaths(directoriesToDelete, _replicaPath));
        return new OperationFileProcessorResult(deleteDirResult, "Directory Deletion");
    }

    /// <summary>
    /// Creates specified directories.
    /// </summary>
    /// <param name="directoriesToCreate">List of directories to create.</param>
    /// <returns>The result of the directory creation operation.</returns>
    private OperationFileProcessorResult CreateDirectories(List<string> directoriesToCreate)
    {
        _logger.Debug("Starting creating directories...");
        var createDirResult = _fileProcessor.CreateDirectories(FileSyncHelper.GetAbsolutePaths(directoriesToCreate, _replicaPath));
        return new OperationFileProcessorResult(createDirResult, "Directory Creation");
    }
    /// <summary>
    /// Moves specified files.
    /// </summary>
    /// <param name="filesToMove">Pairs of initial and final paths for moving files.</param>
    /// <returns>The result of the move operation.</returns>
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

    /// <summary>
    /// Checks if any operation resulted in failures.
    /// </summary>
    /// <param name="results">List of operation results.</param>
    /// <returns>True if there are any failures, false otherwise.</returns>
    private bool AnyFailures(List<OperationFileProcessorResult> results)
    {
        return results.Any(r => r.HasFailures());
    }
}
