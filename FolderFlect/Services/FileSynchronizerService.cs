using FolderFlect.Config;
using FolderFlect.Logging;
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

    public Result DeleteFilesFromDestination(List<string> pathsToDelete)
    {
        foreach (var relativePath in pathsToDelete)
        {
            var fullPathToDelete = Path.Combine(_replicaPath, relativePath);
            if (File.Exists(fullPathToDelete))
            {
                var file = new FileInfo(fullPathToDelete);
                if (file.IsReadOnly)
                {
                    file.IsReadOnly = false;
                }

                try
                {
                    file.Delete();
                    _logger.Log($"File {fullPathToDelete} successfully deleted.");
                }
                catch (Exception e)
                {
                    _logger.Log($"Error deleting file {fullPathToDelete}. Error: {e.Message}");
                    return Result.Fail($"Error deleting file {fullPathToDelete}. Error: {e.Message}");
                }
            }
        }

        return Result.Success();
    }

    public Result CopyFilesToDestination(List<string> pathsToCopy)
    {
        foreach (var relativePath in pathsToCopy)
        {
            var sourceFilePath = Path.Combine(_sourcePath, relativePath);
            var destinationFilePath = Path.Combine(_replicaPath, relativePath);

            if (File.Exists(destinationFilePath))
            {
                var destFile = new FileInfo(destinationFilePath);
                if (destFile.IsReadOnly)
                {
                    destFile.IsReadOnly = false;
                }
            }

            if (File.Exists(sourceFilePath))
            {
                string destinationFolder = Path.GetDirectoryName(destinationFilePath);
                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                try
                {
                    File.Copy(sourceFilePath, destinationFilePath, overwrite: true);

                    // Copying the file attributes
                    File.SetAttributes(destinationFilePath, File.GetAttributes(sourceFilePath));

                    _logger.Log($"File {sourceFilePath} successfully copied to {destinationFilePath}.");
                }
                catch (Exception e)
                {
                    _logger.Log($"Error copying file {sourceFilePath} to {destinationFilePath}. Error: {e.Message}");
                    return Result.Fail($"Error copying file {sourceFilePath} to {destinationFilePath}. Error: {e.Message}");
                }
            }
        }

        return Result.Success();
    }


    public Result UpdateFileAttributes(List<string> pathsToUpdate)
    {
        foreach (var relativePath in pathsToUpdate)
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

                try
                {
                    var sourceFileInfo = new FileInfo(sourceFilePath);

                    destFileInfo.CreationTime = sourceFileInfo.CreationTime;
                    destFileInfo.LastWriteTime = sourceFileInfo.LastWriteTime;
                    destFileInfo.LastAccessTime = sourceFileInfo.LastAccessTime;

                    if (destFileInfo.Attributes != sourceFileInfo.Attributes)
                    {
                        destFileInfo.Attributes = sourceFileInfo.Attributes;
                    }

                    _logger.Log($"Attributes of file {destinationFilePath} successfully updated.");
                }
                catch (Exception e)
                {
                    _logger.Log($"Error updating attributes for file {destinationFilePath}. Error: {e.Message}");
                    return Result.Fail($"Error updating attributes for file {destinationFilePath}. Error: {e.Message}");
                }
            }
        }

        return Result.Success();
    }
}
