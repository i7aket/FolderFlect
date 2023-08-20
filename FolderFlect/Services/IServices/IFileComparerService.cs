using FolderFlect.Models;
using FolderFlect.Utilities;
namespace FolderFlect.Services.IServices;

public interface IFileComparerService
{
    Task<Result<FilesToSyncSetByMD5>> GetFilesToSyncGroupedByMD5AndDirectoryPathsAsync(MD5FileSet fileSet);
}