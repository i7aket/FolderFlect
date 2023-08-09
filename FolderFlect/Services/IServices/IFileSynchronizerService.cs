using FolderFlect.Utilities;

public interface IFileSynchronizerService
{
    Result CopyFilesToDestination(List<string> pathsToCopy);
    Result DeleteFilesFromDestination(List<string> pathsToDelete);
    Result UpdateFileAttributes(List<string> pathsToUpdate);
}