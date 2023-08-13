using FolderFlect.Models;
using FolderFlect.Utilities;
using NLog;

/// <summary>
/// Service responsible for comparing files based on their MD5 hashes and directory paths.
/// </summary>
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

    /// <summary>
    /// Determines groups of files to sync based on their MD5 and directory paths.
    /// Analyses source and destination files to decide on deletion, copying, and moving actions.
    /// </summary>
    /// <param name="fileSet">Set of files with MD5 hashes for analysis.</param>
    /// <returns>Result of file synchronization.</returns>

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

    /// <summary>
    /// Determines directories for deletion based on MD5 comparison.
    /// If a path exists in the destination but not in the source, it's marked for deletion.
    /// </summary>
    /// <param name="directorySet">Set of directories for analysis.</param>
    /// <returns>List of directories intended for deletion.</returns>
    private List<string> DetermineDirectoriesToDeleteMD5(MD5FileSet directorySet)
    {
        _logger.Debug("Determine directories to delete...");
        return directorySet.DestinationDirectories.Keys.Except(directorySet.SourceDirectories.Keys).ToList();
    }

    /// <summary>
    /// Determines directories for creation based on MD5 comparison.
    /// If a path exists in the source but not in the destination, it's marked for creation.
    /// </summary>
    /// <param name="directorySet">Set of directories for analysis.</param>
    /// <returns>List of directories intended for creation.</returns>
    private List<string> DetermineDirectoriesToCreateMD5(MD5FileSet directorySet)
    {
        _logger.Debug("Determine directories to create...");
        return directorySet.SourceDirectories.Keys.Except(directorySet.DestinationDirectories.Keys).ToList();
    }

    /// <summary>
    /// Determines files for deletion based on MD5 comparison.
    /// If an MD5 hash exists in the destination but not in the source, its associated file is marked for deletion.
    /// </summary>
    /// <param name="fileSet">Set of files with MD5 hashes for analysis.</param>
    /// <returns>List of files intended for deletion.</returns>
    private List<string> DetermineFilesToDeleteMD5(MD5FileSet fileSet)
    {
        _logger.Debug("Determine files to delete...");
        return fileSet.DestinationFiles
                      .Where(destFile => !fileSet.SourceFiles.Contains(destFile.Key))
                      .SelectMany(destGroup => destGroup.Select(file => file.FileReletivePath))
                      .ToList();
    }

    /// <summary>
    /// Determines files for copying based on MD5 comparison.
    /// If an MD5 hash exists in the source but not in the destination, its associated file is marked for copying.
    /// </summary>
    /// <param name="fileSet">Set of files with MD5 hashes for analysis.</param>
    /// <returns>List of files intended for copying.</returns>
    private List<string> DetermineFilesToCopyMD5(MD5FileSet fileSet)
    {
        _logger.Debug("Determine files to copy...");
        return fileSet.SourceFiles
                      .Where(srcFile => !fileSet.DestinationFiles.Contains(srcFile.Key))
                      .SelectMany(srcGroup => srcGroup.Select(file => file.FileReletivePath))
                      .ToList();
    }

    /// <summary>
    /// Determines the list of file pairs that need to be moved based on their MD5 hashes.
    /// </summary>
    /// <param name="fileSet">The set of MD5 hashed files to be analyzed.</param>
    /// <param name="filesToDeleteMD5">List of files to be deleted based on the MD5 comparison.</param>
    /// <param name="filesToCopyMD5">List of files to be copied based on the MD5 comparison.</param>
    /// <returns>List of tuples indicating files to move.</returns>
    /// <remarks>
    /// Steps:
    /// 1. Determine intersecting keys between source and destination files.
    /// 2. Create dictionaries for source and destination file paths:
    ///     - For each intersecting key, extract the relative file paths from the source files.
    ///     - Map each source file's relative path to its corresponding key, forming a dictionary.
    ///     - Similarly, for each intersecting key, extract the relative file paths from the destination files.
    ///     - Map each destination file's relative path to its corresponding key, forming a separate dictionary.
    /// 3. Remove entries from both dictionaries where source and destination have matching paths and keys,
    ///      - those files remain unchanged between the source and destination.
    /// 4. Balance the count of source and destination paths: 
    /// 4.1 If there are more source paths than destination, files are marked for copying.
    /// 4.2 If there are more destination paths than source, files are marked for deletion.
    /// 5. Create a list of file pairs indicating the files to be moved.
    /// </remarks>
    private List<(string, string)> DetermineFilesToMoveMD5(MD5FileSet fileSet, List<string> filesToDeleteMD5, List<string> filesToCopyMD5)
    {
        _logger.Debug("Determine files to move...");

        // 1. Determine intersecting keys between source and destination files.
        var listKeys = DetermineIntersectingKeys(fileSet);

        /// 2. Create dictionaries for source and destination file paths:
        ///     - For each intersecting key, extract the relative file paths from the source files.
        ///     - Map each source file's relative path to its corresponding key, forming a dictionary.
        ///     - Similarly, for each intersecting key, extract the relative file paths from the destination files.
        ///     - Map each destination file's relative path to its corresponding key, forming a separate dictionary.
        Dictionary<string, string> SourcePath = listKeys
            .SelectMany(key => fileSet.SourceFiles[key].Select(file => new { file.FileReletivePath, key }))
            .ToDictionary(x => x.FileReletivePath, x => x.key);

        Dictionary<string, string> RelativePath = listKeys
            .SelectMany(key => fileSet.DestinationFiles[key].Select(file => new { file.FileReletivePath, key }))
            .ToDictionary(x => x.FileReletivePath, x => x.key);

        // 3. Remove entries from both dictionaries where source and destination have matching paths and keys.
        //      - those files remain unchanged between the source and destination.
        foreach (var pair in SourcePath.ToList())
        {
            if (RelativePath.ContainsKey(pair.Key) && RelativePath[pair.Key] == pair.Value)
            {
                SourcePath.Remove(pair.Key);
                RelativePath.Remove(pair.Key);
            }
        }

        // 4. Balance the count of source and destination paths.
        var sourcePaths = SourcePath.Keys.ToList();
        var destPaths = RelativePath.Keys.ToList();

        /// 4.1 If there are more source paths than destination, files are marked for copying.
        while (sourcePaths.Count > destPaths.Count)
        {
            filesToCopyMD5.Add(sourcePaths.Last());
            sourcePaths.RemoveAt(sourcePaths.Count - 1);
        }

        /// 4.2 If there are more destination paths than source, files are marked for deletion.
        while (sourcePaths.Count < destPaths.Count)
        {
            filesToDeleteMD5.Add(destPaths.Last());
            destPaths.RemoveAt(destPaths.Count - 1);
        }

        // 5. Create a list of file pairs indicating the files to be moved.
        var filesToMove = new List<(string, string)>();
        for (int i = 0; i < sourcePaths.Count; i++)
        {
            filesToMove.Add((destPaths[i], sourcePaths[i]));
        }

        return filesToMove;
    }


    /// <summary>
    /// Identifies and returns the intersecting MD5 hash keys between the source and destination file sets.
    /// </summary>
    /// <param name="fileSet">The set of MD5 hashed files to be analyzed.</param>
    /// <returns>List of intersecting MD5 hash keys.</returns>
    private List<string> DetermineIntersectingKeys(MD5FileSet fileSet)
    {
        _logger.Debug("Determine intersecting keys...");
        return fileSet.SourceFiles.Select(group => group.Key)
                   .Intersect(fileSet.DestinationFiles.Select(group => group.Key))
                   .ToList();
    }

}
