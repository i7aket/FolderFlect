using FolderFlect.Models;
using FolderFlect.Utilities;

public interface IFileSynchronizerService
{
    void MoveFiles(List<(string, string)> filesToMove);
    Result SyncFilesByMD5(FilesToSyncSetByMD5 filesToSyncSet);
}