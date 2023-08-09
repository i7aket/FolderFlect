using FolderFlect.Models;
using FolderFlect.Utilities;

public interface IFileComparerService
{
    Result<List<string>> GetFilesForAttributesUpdate(List<string> intersectingFiles, Dictionary<string, FileModel> sourceFiles, Dictionary<string, FileModel> destFiles);
    Result<List<string>> GetFilesForUpdate(List<string> intersectingFiles, Dictionary<string, FileModel> sourceFiles, Dictionary<string, FileModel> destFiles);
    Result<List<string>> GetFilesToCopy(Dictionary<string, FileModel> sourceFiles, Dictionary<string, FileModel> destFiles);
    Result<List<string>> GetFilesToDeleteFromDestination(Dictionary<string, FileModel> sourceFiles, Dictionary<string, FileModel> destFiles);
    Result<List<string>> GetIntersectingFiles(Dictionary<string, FileModel> sourceFiles, Dictionary<string, FileModel> destFiles);
}