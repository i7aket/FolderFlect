using FolderFlect.Models;
using FolderFlect.Utilities;

namespace FolderFlect.Services.IServices
{
    public interface IFileScannerService
    {
        Task<Result<MD5FileSet>> RetrieveFilesGroupedByMD5AndDirectoryPathsAsync();
    }
}