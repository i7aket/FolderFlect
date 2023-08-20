using FolderFlect.Models;

namespace FolderFlect.Services.IServices;

public interface IFileProcessorService
{
    Task<FileProcessorResult> CopyFilesAsync(List<(string SourcePath, string DestinationPath)> absolutePathsToCopy);
    Task<FileProcessorResult> CreateDirectoriesAsync(List<string> absolutePathsToCreate);
    Task<FileProcessorResult> DeleteDirectoriesAsync(List<string> absolutePathsToDelete);
    Task<FileProcessorResult> DeleteFilesAsync(List<string> absolutePathsToDelete);
    Task<FileProcessorResult> MoveFilesAsync(List<(string SourcePath, string DestinationPath)> absolutePathsToMove);
}

