using FolderFlect.Config;
using FolderFlect.Models;
using FolderFlect.Services.IServices;
using FolderFlect.Utilities;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using static FolderFlect.Constants.Descriptor;

namespace FolderFlect.Services
{
    public class FileScannerService : IFileScannerService
    {
        #region Fields and Constructor


        private readonly ILogger _logger;
        private readonly string _sourcePath;
        private readonly string _replicaPath;

        public FileScannerService(ILogger logger, AppConfig appConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sourcePath = appConfig.SourcePath;
            _replicaPath = appConfig.ReplicaPath;
            _logger.Debug($"Initialized FileScannerService with SourcePath: {_sourcePath} and ReplicaPath: {_replicaPath}");
        }

        #endregion


        public Result<MD5FileSet> RetrieveFilesGroupedByMD5AndDirectoryPaths()
        {
            _logger.Debug("Starting RetrieveFilesGroupedByMD5AndDirectoryPaths");
            try
            {
                var sourceFilesByMD5Hash = ScanDirectoryForFilesByMD5Hash(_sourcePath, Source_Descriptor);
                var destForFilesByMD5Hash = ScanDirectoryForFilesByMD5Hash(_replicaPath, Destination_Descriptor);
                var sourceDirectoriesByRelativePath = ScanDirectoriesByRelativePath(_sourcePath, Source_Descriptor);
                var destDirectoriesByRelativePath = ScanDirectoriesByRelativePath(_replicaPath, Destination_Descriptor);

                var fileSet = new MD5FileSet(
                    sourceFilesByMD5Hash,
                    destForFilesByMD5Hash,
                    sourceDirectoriesByRelativePath,
                    destDirectoriesByRelativePath
                );

                _logger.Debug("Finished RetrieveFilesGroupedByMD5AndDirectoryPaths with success.");
                return Result<MD5FileSet>.Success(fileSet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error retrieving all relative file paths.");
                return Result<MD5FileSet>.Fail("Error retrieving all relative file paths.");
            }
        }


        private void EnsureDirectoryExists(string directoryPath)
        {

            if (!Directory.Exists(directoryPath))
            {
                try
                {
                    Directory.CreateDirectory(directoryPath);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Failed to create directory at: {directoryPath}");
                    throw; 
                }
            }

        }


        private Dictionary<string, string> ScanDirectoriesByRelativePath(string directoryPath, string descriptor)
        {
            _logger.Debug($"Starting scanning for {descriptor} directories by relative path at {directoryPath}");

            EnsureDirectoryExists(directoryPath);
            var directories = Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories)
                                       .ToDictionary(dir => PathHelper.GetRelativePath(directoryPath, dir), dir => dir);

            return directories;
        }

        private FileModel CreateFileModel(string absoluteFilePath, string baseDirectory)
        {
            try
            {
                var fileInfo = new FileInfo(absoluteFilePath);
                var model = new FileModel
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
                return model;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error in CreateFileModel for absoluteFilePath: {absoluteFilePath}");
                return null;
            }
        }

        private string CalculateMD5(string filePath)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var hash = md5.ComputeHash(fileStream);
                        var result = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Error in CalculateMD5 for filePath: {filePath}");
                throw; 
            }
        }


        private ILookup<string, FileModel> ScanDirectoryForFilesByMD5Hash(string directoryPath, string descriptor)
        {
            _logger.Debug($"Starting scanning for {descriptor} files by MD5 hash at {directoryPath}");

            EnsureDirectoryExists(directoryPath);
            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)
                                 .Select(file => CreateFileModel(file, directoryPath))
                                 .Where(fileModel => fileModel != null)
                                 .ToLookup(fileModel => fileModel.MD5Hash);

            return files;
        }

    }
}
