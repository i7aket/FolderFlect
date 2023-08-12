using FolderFlect.Models;
using FolderFlect.Utilities;

public interface IFileSynchronizerService
{
    Result SyncFilesByMD5(FilesToSyncSetByMD5 filesToSyncSet);
}