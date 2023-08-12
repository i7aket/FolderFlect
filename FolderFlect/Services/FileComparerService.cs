using FolderFlect.Config;
using NLog;
using FolderFlect.Models;
using FolderFlect.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using NLog.Layouts;
using Microsoft.VisualBasic;

public class FileComparerService : IFileComparerService
{

    #region Fields and Constructor

    private readonly ILogger _logger;

    public FileComparerService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.Debug("Initialized FileComparerService.");

    }
    #endregion

    public Result<FilesToSyncSetByMD5> GetFilesToSyncGroupedByMD5AndDirectoryPaths(MD5FileSet fileSet)
    {
        _logger.Debug("Starting GetFilesToSyncGroupedByMD5AndDirectoryPaths");
        try
        {
            var directoriesToDeleteMD5 = GetDirectoriesToDeleteMD5(fileSet);
            var directoriesToCreateMD5 = GetDirectoriesToCreateMD5(fileSet);
            var filesToDeleteMD5 = GetFilesToDeleteMD5(fileSet);
            var filesToCopyMD5 = GetFilesToCopyMD5(fileSet);
            var filesToMoveMD = GetFilesToMoveMD5(fileSet, filesToDeleteMD5, filesToCopyMD5);

            FilesToSyncSetByMD5 filesToSyncSet = new FilesToSyncSetByMD5(
                directoriesToDeleteMD5,                     //DirectoriesToDelete 
                directoriesToCreateMD5,                     //FilesToDelete 
                filesToDeleteMD5,                           //FilesToCopy 
                filesToCopyMD5,                             //DirectoriesToCreate 
                filesToMoveMD                               //FilesForUpdate 
            );
            _logger.Debug("Finished GetFilesToSyncGroupedByMD5AndDirectoryPaths with success.");
            return Result<FilesToSyncSetByMD5>.Success(filesToSyncSet);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error during files synchronization: {ex.Message}");
            return Result<FilesToSyncSetByMD5>.Fail($"Error during files synchronization: {ex.Message}");
        }
    }

    private List<string> GetDirectoriesToDeleteMD5(MD5FileSet directorySet)
    {
        _logger.Debug("Determining directories to delete...");
        return directorySet.DestinationDirectories.Keys.Except(directorySet.SourceDirectories.Keys).ToList();
    }

    private List<string> GetDirectoriesToCreateMD5(MD5FileSet directorySet)
    {
        _logger.Debug("Determining directories to create...");
        return directorySet.SourceDirectories.Keys.Except(directorySet.DestinationDirectories.Keys).ToList();
    }

    private List<string> GetFilesToDeleteMD5(MD5FileSet fileSet)
    {
        _logger.Debug("Determining files to delete...");
        return fileSet.DestinationFiles
                      .Where(destGroup => !fileSet.SourceFiles.Any(srcGroup => srcGroup.Key == destGroup.Key))
                      .SelectMany(destGroup => destGroup.Select(file => file.FileReletivePath))
                      .ToList();
    }

    private List<string> GetFilesToCopyMD5(MD5FileSet fileSet)
    {
        _logger.Debug("Determining files to copy...");
        return fileSet.SourceFiles
                      .Where(srcGroup => !fileSet.DestinationFiles.Any(destGroup => destGroup.Key == srcGroup.Key))
                      .SelectMany(srcGroup => srcGroup.Select(file => file.FileReletivePath))
                      .ToList();
    }

    private List<(string, string)> GetFilesToMoveMD5(MD5FileSet fileSet, List<string> filesToDeleteMD5, List<string> filesToCopyMD5)
    {
        _logger.Debug("Determining files to move...");

        var listKeys = GetIntersectingKeys(fileSet);

        Dictionary<string, string> SourcePath = listKeys
            .SelectMany(key => fileSet.SourceFiles[key].Select(file => new { file.FileReletivePath, key }))
            .ToDictionary(x => x.FileReletivePath, x => x.key);

        Dictionary<string, string> RelativePath = listKeys
            .SelectMany(key => fileSet.DestinationFiles[key].Select(file => new { file.FileReletivePath, key }))
            .ToDictionary(x => x.FileReletivePath, x => x.key);

        foreach (var pair in SourcePath.ToList())
        {
            if (RelativePath.ContainsKey(pair.Key) && RelativePath[pair.Key] == pair.Value)
            {
                SourcePath.Remove(pair.Key);
                RelativePath.Remove(pair.Key);
            }
        }

        var filesToMove = new List<(string, string)>();
        var sourcePaths = SourcePath.Keys.ToList();
        var destPaths = RelativePath.Keys.ToList();

        while (sourcePaths.Count > destPaths.Count)
        {
            filesToCopyMD5.Add(sourcePaths.Last());
            sourcePaths.RemoveAt(sourcePaths.Count - 1);
        }

        while (sourcePaths.Count < destPaths.Count)
        {
            filesToDeleteMD5.Add(destPaths.Last());
            destPaths.RemoveAt(destPaths.Count - 1);
        }

        for (int i = 0; i < sourcePaths.Count; i++)
        {
            filesToMove.Add((destPaths[i], sourcePaths[i]));
        }

        return filesToMove;
    }

    private List<string> GetIntersectingKeys(MD5FileSet fileSet)
    {
        _logger.Debug("Determining intersecting keys...");
        var sourceKeys = fileSet.SourceFiles.Select(group => group.Key).ToList();
        var destinationKeys = fileSet.DestinationFiles.Select(group => group.Key).ToList();

        return sourceKeys.Intersect(destinationKeys).ToList();
    }


}
