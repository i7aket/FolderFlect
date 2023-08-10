namespace FolderFlect.Models
{
    public record FileModel
    {
        public string FileName { get; init; }
        public string FilePath { get; init; }
        public string FileReletivePath { get; init; }
        public long FileSize { get; init; }
        public string MD5Hash { get; init; }
        public DateTime CreationTime { get; init; }
        public DateTime LastModifiedTime { get; init; }
        public DateTime LastAccessTime { get; init; }
        public FileAttributes FileSystemAttributes { get; init; }
    }
}