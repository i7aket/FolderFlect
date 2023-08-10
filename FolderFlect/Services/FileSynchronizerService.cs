using FolderFlect.Config;
using NLog;
using FolderFlect.Models;
using FolderFlect.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

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
    }

    public Result SyncFiles(FilesToSyncSet filesToSyncSet)
    {
        DeleteFilesFromDestination(filesToSyncSet.FilesToDelete);
        DeleteDirectories(filesToSyncSet.DirectoriesToDelete);
        CreateDirectories(filesToSyncSet.DirectoriesToCreate);
        CopyFilesToDestination(filesToSyncSet.FilesToCopy);
        CopyFilesToDestination(filesToSyncSet.FilesForUpdate);
        UpdateFileAttributes(filesToSyncSet.FilesForAttributesUpdate);

        return Result.Success();
    }

    private void DeleteFilesFromDestination(IEnumerable<string> pathsToDelete)

    {
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
    }

    private void CopyFilesToDestination(IEnumerable<string> pathsToCopy)
    {
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

                    File.Copy(sourceFilePath, destinationFilePath, overwrite: true);
                    File.SetAttributes(destinationFilePath, File.GetAttributes(sourceFilePath));
                    _logger.Info($"File {sourceFilePath} successfully copied to {destinationFilePath}.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error copying file {relativePath}.");
            }
        }
    }

    private void UpdateFileAttributes(List<string> pathsToUpdate)
    {
        foreach (var relativePath in pathsToUpdate)
        {
            try
            {
                var sourceFilePath = Path.Combine(_sourcePath, relativePath);
                var destinationFilePath = Path.Combine(_replicaPath, relativePath);

                if (File.Exists(sourceFilePath) && File.Exists(destinationFilePath))
                {
                    var destFileInfo = new FileInfo(destinationFilePath);
                    if (destFileInfo.IsReadOnly)
                    {
                        destFileInfo.IsReadOnly = false;
                    }

                    var sourceFileInfo = new FileInfo(sourceFilePath);

                    destFileInfo.CreationTime = sourceFileInfo.CreationTime;
                    destFileInfo.LastWriteTime = sourceFileInfo.LastWriteTime;
                    destFileInfo.LastAccessTime = sourceFileInfo.LastAccessTime;

                    if (destFileInfo.Attributes != sourceFileInfo.Attributes)
                    {
                        destFileInfo.Attributes = sourceFileInfo.Attributes;
                    }

                    _logger.Info($"Attributes of file {destinationFilePath} successfully updated.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error updating attributes for file {relativePath}.");
            }
        }
    }

    private void DeleteDirectories(List<string> directoriesToDelete)
    {
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
    }

    private void CreateDirectories(List<string> directoriesToCreate)
    {
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
    }

    private void SetFileAsWritable(FileInfo file)
    {
        if (file.IsReadOnly)
            file.IsReadOnly = false;
    }
}
