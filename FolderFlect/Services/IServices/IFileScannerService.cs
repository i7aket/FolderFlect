using FolderFlect.Models;
using FolderFlect.Utilities;

namespace FolderFlect.Services.IServices
{
    public interface IFileScannerService
    {
        Result<DirectoryFileSet> GetAllRelativeFilePaths();
    }
}