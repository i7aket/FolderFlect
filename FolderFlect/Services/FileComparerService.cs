using FolderFlect.Config;
using FolderFlect.Logging;
using FolderFlect.Models;
using FolderFlect.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

public class FileComparerService : IFileComparerService
{
    private readonly ILogger _logger;

    public FileComparerService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Result<List<string>> GetFilesForUpdate(
        List<string> intersectingFiles,
        Dictionary<string, FileModel> sourceFiles,
        Dictionary<string, FileModel> destFiles)
    {
        try
        {
            var pathsForFullCopy = new List<string>();

            foreach (var path in intersectingFiles)
            {
                var sourceFile = sourceFiles[path];
                var destFile = destFiles[path];

                if (sourceFile.MD5Hash != destFile.MD5Hash)
                {
                    pathsForFullCopy.Add(path);
                }
            }

            return Result<List<string>>.Success(pathsForFullCopy);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error in GetFilesForUpdate: {ex.Message}");
            return Result<List<string>>.Fail($"Error in GetFilesForUpdate: {ex.Message}");
        }
    }

    public Result<List<string>> GetFilesForAttributesUpdate(
        List<string> intersectingFiles,
        Dictionary<string, FileModel> sourceFiles,
        Dictionary<string, FileModel> destFiles)
    {
        try
        {
            var pathsForAttributesUpdate = new List<string>();

            foreach (var path in intersectingFiles)
            {
                var sourceFile = sourceFiles[path];
                var destFile = destFiles[path];

                if (sourceFile.MD5Hash == destFile.MD5Hash &&
                    sourceFile.FileSystemAttributes != destFile.FileSystemAttributes)
                {
                    pathsForAttributesUpdate.Add(path);
                }
            }

            return Result<List<string>>.Success(pathsForAttributesUpdate);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error in GetFilesForAttributesUpdate: {ex.Message}");
            return Result<List<string>>.Fail($"Error in GetFilesForAttributesUpdate: {ex.Message}");
        }
    }

    public Result<List<string>> GetIntersectingFiles(
        Dictionary<string, FileModel> sourceFiles,
        Dictionary<string, FileModel> destFiles)
    {
        try
        {
            var intersectingFiles = sourceFiles.Keys.Intersect(destFiles.Keys).ToList();
            return Result<List<string>>.Success(intersectingFiles);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error in GetIntersectingFiles: {ex.Message}");
            return Result<List<string>>.Fail($"Error in GetIntersectingFiles: {ex.Message}");
        }
    }

    public Result<List<string>> GetFilesToDeleteFromDestination(
        Dictionary<string, FileModel> sourceFiles,
        Dictionary<string, FileModel> destFiles)
    {
        try
        {
            var pathsToDelete = new List<string>();

            // Iterate through all paths in the destination folder.
            foreach (var destKvp in destFiles)
            {
                // If such a path is not present in the source folder, add the path to the delete list.
                if (!sourceFiles.ContainsKey(destKvp.Key))
                {
                    pathsToDelete.Add(destKvp.Key);
                }
                else
                {
                    // If the path is present in both the source and destination folders, 
                    // remove the path from the delete list if the file in the source has the same name.
                    var sourceFileName = sourceFiles[destKvp.Key].FileName;
                    if (!destKvp.Value.FileName.Equals(sourceFileName))
                    {
                        pathsToDelete.Add(destKvp.Key);
                    }
                }
            }

            return Result<List<string>>.Success(pathsToDelete);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error in GetFilesToDeleteFromDestination: {ex.Message}");
            return Result<List<string>>.Fail($"Error in GetFilesToDeleteFromDestination: {ex.Message}");
        }
    }

    public Result<List<string>> GetFilesToCopy(
        Dictionary<string, FileModel> sourceFiles,
        Dictionary<string, FileModel> destFiles)
    {
        try
        {
            var pathsToCopy = new List<string>();

            // Iterate through all paths in the source folder.
            foreach (var sourceKvp in sourceFiles)
            {
                // If such a path (relative, including the file name) is not in the destination folder, add the path to the copy list.
                if (!destFiles.ContainsKey(sourceKvp.Key))
                {
                    pathsToCopy.Add(sourceKvp.Key);
                }
            }

            return Result<List<string>>.Success(pathsToCopy);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error in GetFilesToCopy: {ex.Message}");
            return Result<List<string>>.Fail($"Error in GetFilesToCopy: {ex.Message}");
        }
    }
}
