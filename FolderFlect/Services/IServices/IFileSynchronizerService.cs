using FolderFlect.Models;
using FolderFlect.Utilities;

public interface IFileSynchronizerService
{
    Result SyncFiles(FilesToSyncSet filesToSyncSet);
}