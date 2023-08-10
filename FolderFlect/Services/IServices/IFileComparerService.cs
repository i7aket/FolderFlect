using FolderFlect.Models;
using FolderFlect.Utilities;

public interface IFileComparerService
{
    Result<FilesToSyncSet> GetFilesToSync(DirectoryFileSet fileSet);
}