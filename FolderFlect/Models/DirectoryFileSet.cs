namespace FolderFlect.Models
{
    public record DirectoryFileSet(
        Dictionary<string, FileModel> SourceFiles,
        Dictionary<string, FileModel> DestenationFiles,
        Dictionary<string, string> SourceDirectories,
        Dictionary<string, string> DestenationDirectories);
}