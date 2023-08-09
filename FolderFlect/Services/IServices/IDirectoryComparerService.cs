using FolderFlect.Utilities;

namespace FolderFlect.Services.IServices
{
    public interface IDirectoryComparerService
    {
        Result<List<string>> GetDirectoriesToCreate(Dictionary<string, string> sourceDirectory, Dictionary<string, string> destDirectory);
        Result<List<string>> GetDirectoriesToDelete(Dictionary<string, string> sourceDirectory, Dictionary<string, string> destDirectory);
    }
}