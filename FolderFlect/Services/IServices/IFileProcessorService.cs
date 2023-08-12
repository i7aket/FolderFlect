using FolderFlect.Models;

namespace FolderFlect.Services.IServices
{
    public interface IFileProcessorService
    {
        FileProcessorResult CopyFiles(List<(string SourcePath, string DestinationPath)> absolutePathsToCopy);
        FileProcessorResult CreateDirectories(List<string> absolutePathsToCreate);
        FileProcessorResult DeleteDirectories(List<string> absolutePathsToDelete);
        FileProcessorResult DeleteFiles(List<string> absolutePathsToDelete);
        FileProcessorResult MoveFiles(List<(string SourcePath, string DestinationPath)> absolutePathsToMove);
    }
}