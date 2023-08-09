using FolderFlect.Utilities;

public interface IDirectoryScannerService
{
    Result<Dictionary<string, string>> ScanDestinationDirectoryByRelativePath();
    Result<Dictionary<string, string>> ScanSourceDirectoryByRelativePath();
}