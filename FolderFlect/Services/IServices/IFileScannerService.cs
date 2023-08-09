using FolderFlect.Models;
using FolderFlect.Utilities;

namespace FolderFlect.Services.IServices
{
    public interface IFileScannerService
    {
        Result<Dictionary<string, FileModel>> ScanDestinationDirectoryByRelativePath();
        Result<Dictionary<string, List<FileModel>>> ScanDirectory(string directoryPath);
        Result<Dictionary<string, FileModel>> ScanSourceDirectoryByRelativePath();
    }
}