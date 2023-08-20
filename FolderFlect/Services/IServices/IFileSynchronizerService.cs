using FolderFlect.Models;
using FolderFlect.Utilities;
namespace FolderFlect.Services.IServices;

public interface IFileSynchronizerService
{
    Task<Result> SyncFilesByMD5(FilesToSyncSetByMD5 filesToSyncSet);
}