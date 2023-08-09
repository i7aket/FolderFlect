using FolderFlect.Utilities;

namespace FolderFlect.Services.IServices
{
    public interface IDirectorySynchronizerService
    {
        Result CreateDirectories(List<string> directoriesToCreate);
        Result DeleteDirectories(List<string> directoriesToDelete);
    }
}