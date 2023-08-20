namespace FolderFlect.Models;

public record MD5FileSet(
    ILookup<string, FileModel> SourceFiles,
    ILookup<string, FileModel> DestinationFiles,
    Dictionary<string, string> SourceDirectories,
    Dictionary<string, string> DestinationDirectories);


