using FolderFlect.Models;
using FolderFlect.Utilities;

public interface IFileComparerService
{
    Result<FilesToSyncSetByMD5> GetFilesToSyncGroupedByMD5AndDirectoryPaths(MD5FileSet fileSet);
}