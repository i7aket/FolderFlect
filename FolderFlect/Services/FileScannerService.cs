using FolderFlect.Config;
using FolderFlect.Models;
using FolderFlect.Services.IServices;
using FolderFlect.Utilities;
using NLog;
using System.Security.Cryptography;

namespace FolderFlect.Services
{
    public class FileScannerService : IFileScannerService
    {
        private readonly ILogger _logger;
        private readonly string _sourcePath;
        private readonly string _replicaPath;

        public FileScannerService(ILogger logger, AppConfig appConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sourcePath = appConfig.SourcePath;
            _replicaPath = appConfig.ReplicaPath;
        }

        public Result<DirectoryFileSet> GetAllRelativeFilePaths()
        {
            try
            {
                var directoryFileSet = new DirectoryFileSet(
                    ScanDirectoryForFilesByRelativePath(_sourcePath),
                    ScanDirectoryForFilesByRelativePath(_replicaPath),
                    ScanDirectoriesByRelativePath(_sourcePath),
                    ScanDirectoriesByRelativePath(_replicaPath)
                );
                return Result<DirectoryFileSet>.Success(directoryFileSet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error retrieving all relative file paths.");
                return Result<DirectoryFileSet>.Fail("Error retrieving all relative file paths.");
            }
        }

        private void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private Dictionary<string, string> ScanDirectoriesByRelativePath(string directoryPath)
        {
            EnsureDirectoryExists(directoryPath);

            return Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories)
                            .ToDictionary(dir => PathHelper.GetRelativePath(directoryPath, dir), dir => dir);
        }

        private Dictionary<string, FileModel> ScanDirectoryForFilesByRelativePath(string directoryPath)
        {
            EnsureDirectoryExists(directoryPath);

            return Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)
                            .Select(file => CreateFileModel(file, directoryPath))
                            .ToDictionary(fileModel => fileModel.FileReletivePath, fileModel => fileModel);
        }

        private FileModel CreateFileModel(string absoluteFilePath, string baseDirectory)
        {
            var fileInfo = new FileInfo(absoluteFilePath);
            return new FileModel
            {
                FileName = fileInfo.Name,
                FilePath = absoluteFilePath,
                FileReletivePath = PathHelper.GetRelativePath(baseDirectory, absoluteFilePath),
                FileSize = fileInfo.Length,
                MD5Hash = CalculateMD5(absoluteFilePath),
                CreationTime = fileInfo.CreationTime,
                LastModifiedTime = fileInfo.LastWriteTime,
                LastAccessTime = fileInfo.LastAccessTime,
                FileSystemAttributes = fileInfo.Attributes
            };
        }

        private string CalculateMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
