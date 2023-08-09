namespace FolderFlect.Models
{
    public class FileModel
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileReletivePath { get; set; }
        public long FileSize { get; set; }
        public string MD5Hash { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastModifiedTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public FileAttributes FileSystemAttributes { get; set; }
    }
}