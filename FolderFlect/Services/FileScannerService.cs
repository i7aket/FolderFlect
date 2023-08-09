using FolderFlect.Config;
using FolderFlect.Logging;
using FolderFlect.Models;
using FolderFlect.Services.IServices;
using FolderFlect.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public Result<Dictionary<string, FileModel>> ScanSourceDirectoryByRelativePath()
        {
            try
            {
                return ScanDirectoryByRelativePath(_sourcePath);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in ScanSourceDirectoryByRelativePath: {ex.Message}");
                return Result<Dictionary<string, FileModel>>.Fail($"Error in ScanSourceDirectoryByRelativePath: {ex.Message}");
            }
        }

        public Result<Dictionary<string, FileModel>> ScanDestinationDirectoryByRelativePath()
        {
            try
            {
                return ScanDirectoryByRelativePath(_replicaPath);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in ScanDestinationDirectoryByRelativePath: {ex.Message}");
                return Result<Dictionary<string, FileModel>>.Fail($"Error in ScanDestinationDirectoryByRelativePath: {ex.Message}");
            }
        }

        private Result<Dictionary<string, FileModel>> ScanDirectoryByRelativePath(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    return Result<Dictionary<string, FileModel>>.Success(new Dictionary<string, FileModel>());
                }

                var fileMap = new Dictionary<string, FileModel>();
                var allFiles = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
                foreach (var file in allFiles)
                {
                    var fileModel = CreateFileModel(file, directoryPath);
                    var relativePathDirectory = fileModel.FileReletivePath;
                    fileMap[relativePathDirectory] = fileModel;
                }

                return Result<Dictionary<string, FileModel>>.Success(fileMap);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in ScanDirectoryByRelativePath: {ex.Message}");
                return Result<Dictionary<string, FileModel>>.Fail($"Error in ScanDirectoryByRelativePath: {ex.Message}");
            }
        }

        public Result<Dictionary<string, List<FileModel>>> ScanDirectory(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    return Result<Dictionary<string, List<FileModel>>>.Success(new Dictionary<string, List<FileModel>>());
                }

                var fileMap = new Dictionary<string, List<FileModel>>();
                var allFiles = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
                foreach (var file in allFiles)
                {
                    var fileModel = CreateFileModel(file, directoryPath);
                    if (!fileMap.ContainsKey(fileModel.MD5Hash))
                    {
                        fileMap[fileModel.MD5Hash] = new List<FileModel>();
                    }

                    fileMap[fileModel.MD5Hash].Add(fileModel);
                }

                return Result<Dictionary<string, List<FileModel>>>.Success(fileMap);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in ScanDirectory: {ex.Message}");
                return Result<Dictionary<string, List<FileModel>>>.Fail($"Error in ScanDirectory: {ex.Message}");
            }
        }

        private FileModel CreateFileModel(string absoluteFilePath, string baseDirectory)
        {
            try
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
            catch (Exception ex)
            {
                _logger.Error($"Error in CreateFileModel: {ex.Message}");
                throw;
            }
        }

        private string CalculateMD5(string filePath)
        {
            try
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
            catch (Exception ex)
            {
                _logger.Error($"Error in CalculateMD5 for {filePath}: {ex.Message}");
                throw;
            }
        }
    }
}
