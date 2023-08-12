using FolderFlect.Models;
using FolderFlect.Utilities;
using NLog;

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
            var directoriesToDeleteMD5 = DetermineDirectoriesToDeleteMD5(fileSet);
            var directoriesToCreateMD5 = DetermineDirectoriesToCreateMD5(fileSet);
            var filesToDeleteMD5 = DetermineFilesToDeleteMD5(fileSet);
            var filesToCopyMD5 = DetermineFilesToCopyMD5(fileSet);
            var filesToMoveMD = DetermineFilesToMoveMD5(fileSet, filesToDeleteMD5, filesToCopyMD5);

            FilesToSyncSetByMD5 filesToSyncSet = new FilesToSyncSetByMD5(
                directoriesToDeleteMD5,
                directoriesToCreateMD5,
                filesToDeleteMD5,
                filesToCopyMD5,
                filesToMoveMD
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

    private List<string> DetermineDirectoriesToDeleteMD5(MD5FileSet directorySet)
    {
        _logger.Debug("Determine directories to delete...");
        return directorySet.DestinationDirectories.Keys.Except(directorySet.SourceDirectories.Keys).ToList();
    }

    private List<string> DetermineDirectoriesToCreateMD5(MD5FileSet directorySet)
    {
        _logger.Debug("Determine directories to create...");
        return directorySet.SourceDirectories.Keys.Except(directorySet.DestinationDirectories.Keys).ToList();
    }

    private List<string> DetermineFilesToDeleteMD5(MD5FileSet fileSet)
    {
        _logger.Debug("Determine files to delete...");
        return fileSet.DestinationFiles
                      .Where(destFile => !fileSet.SourceFiles.Contains(destFile.Key))
                      .SelectMany(destGroup => destGroup.Select(file => file.FileReletivePath))
                      .ToList();
    }

    private List<string> DetermineFilesToCopyMD5(MD5FileSet fileSet)
    {
        _logger.Debug("Determine files to copy...");
        return fileSet.SourceFiles
                      .Where(srcFile => !fileSet.DestinationFiles.Contains(srcFile.Key))
                      .SelectMany(srcGroup => srcGroup.Select(file => file.FileReletivePath))
                      .ToList();
    }

    private List<(string, string)> DetermineFilesToMoveMD5(MD5FileSet fileSet, List<string> filesToDeleteMD5, List<string> filesToCopyMD5)
    {
        _logger.Debug("Determine files to move...");

        var listKeys = DetermineIntersectingKeys(fileSet);

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

    private List<string> DetermineIntersectingKeys(MD5FileSet fileSet)
    {
        _logger.Debug("Determine intersecting keys...");
        return fileSet.SourceFiles.Select(group => group.Key)
                   .Intersect(fileSet.DestinationFiles.Select(group => group.Key))
                   .ToList();
    }
}
