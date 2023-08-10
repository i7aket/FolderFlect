using FolderFlect.Config;
using NLog;
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

    public Result<FilesToSyncSet> GetFilesToSync(DirectoryFileSet fileSet)
    {
        try
        {
            var intersectingFiles = GetIntersectingFiles(fileSet);

            FilesToSyncSet filesToSyncSet = new FilesToSyncSet(
                GetDirectoriesToDelete(fileSet),                        //DirectoriesToDelete 
                GetFilesToDeleteFromDestination(fileSet),               //FilesToDelete 
                GetFilesToCopy(fileSet),                                //FilesToCopy 
                GetDirectoriesToCreate(fileSet),                        //DirectoriesToCreate 
                GetFilesForUpdate(intersectingFiles, fileSet),          //FilesForUpdate 
                GetFilesForAttributesUpdate(intersectingFiles, fileSet) //FilesForAttributesUpdate 
            );

            return Result<FilesToSyncSet>.Success(filesToSyncSet);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error during files synchronization: {ex.Message}");
            return Result<FilesToSyncSet>.Fail($"Error during files synchronization: {ex.Message}");
        }
    }


    private List<string> GetIntersectingFiles(DirectoryFileSet fileSet)
    {
        var intersectingFiles = fileSet.SourceFiles.Keys.Intersect(fileSet.DestenationFiles.Keys).ToList();
        return intersectingFiles;
    }

    private List<string> GetDirectoriesToDelete(DirectoryFileSet directorySet)
    {
        return directorySet.DestenationDirectories.Keys.Except(directorySet.SourceDirectories.Keys).ToList();
    }

    private List<string> GetDirectoriesToCreate(DirectoryFileSet directorySet)
    {
        return directorySet.SourceDirectories.Keys.Except(directorySet.DestenationDirectories.Keys).ToList();
    }

    private List<string> GetFilesForUpdate(List<string> intersectingFiles, DirectoryFileSet fileSet)
    {
        return FilterFilesByCriteria(fileSet, intersectingFiles,
            (sourceFile, destFile) => sourceFile.MD5Hash != destFile.MD5Hash);
    }

    private List<string> GetFilesForAttributesUpdate(List<string> intersectingFiles, DirectoryFileSet fileSet)
    {
        return FilterFilesByCriteria(fileSet, intersectingFiles,
            (sourceFile, destFile) => sourceFile.MD5Hash == destFile.MD5Hash && sourceFile.FileSystemAttributes != destFile.FileSystemAttributes);
    }

    private List<string> GetFilesToDeleteFromDestination(DirectoryFileSet fileSet)
    {
        return fileSet.DestenationFiles
            .Where(destKvp =>
                fileSet.SourceFiles.ContainsKey(destKvp.Key)
                ? destKvp.Value.FileName != fileSet.SourceFiles[destKvp.Key].FileName
                : true)
            .Select(destKvp => destKvp.Key)
            .ToList();
    }

    private List<string> FilterFilesByCriteria(DirectoryFileSet fileSet, List<string> intersectingFiles, Func<FileModel, FileModel, bool> criteria)
    {
        return intersectingFiles
               .Where(path => criteria(fileSet.SourceFiles[path], fileSet.DestenationFiles[path]))
               .ToList();
    }

    private List<string> GetFilesToCopy(DirectoryFileSet fileSet)
    {
        return fileSet.SourceFiles.Keys.Except(fileSet.DestenationFiles.Keys).ToList();
    }

}
