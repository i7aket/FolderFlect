namespace FolderFlect.Models;
public record FilesToSyncSetByMD5
{
    public FilesToSyncSetByMD5(
        List<string> directoriesToDelete,
        List<string> directoriesToCreate,

        List<string> filesToDelete,
        List<string> filesToCopy,
        List<(string sourcePath, string destinationPath)> filesToMove)
    {
        DirectoriesToDelete = directoriesToDelete;
        DirectoriesToCreate = directoriesToCreate;

        FilesToDelete = filesToDelete;
        FilesToCopy = filesToCopy;
        FilesToMove = filesToMove;
    }

    public List<string> DirectoriesToDelete { get; init; }
    public List<string> DirectoriesToCreate { get; init; }
    public List<string> FilesToDelete { get; init; }
    public List<string> FilesToCopy { get; init; }
    public List<(string SourcePath, string DestinationPath)> FilesToMove { get; init; }
}

