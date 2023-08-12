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
                var sourceFilesByMD5Hash = ScanDirectory(_sourcePath, Source_Descriptor, FilesByMD5HashProcessor);
                var destForFilesByMD5Hash = ScanDirectory(_replicaPath, Destination_Descriptor, FilesByMD5HashProcessor);
                var sourceDirectoriesByRelativePath = ScanDirectory(_sourcePath, Source_Descriptor, DirectoriesByRelativePathProcessor);
                var destDirectoriesByRelativePath = ScanDirectory(_replicaPath, Destination_Descriptor, DirectoriesByRelativePathProcessor);

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
                string Error = "Error retrieving all relative file paths.";
                _logger.Error(ex, Error);
                return Result<MD5FileSet>.Fail(Error);
            }
        }

        private TResult ScanDirectory<TResult>(string directoryPath, string descriptor, Func<string, TResult> pathProcessor)
        {
            _logger.Debug($"Starting scanning for {descriptor} at {directoryPath}");
            FileSyncHelper.EnsureDirectoryExists(directoryPath);
            return pathProcessor(directoryPath);
        }

        private Dictionary<string, string> DirectoriesByRelativePathProcessor(string directoryPath)
        {
            var directories = Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories)
                                       .ToDictionary(dir => FileSyncHelper.GetRelativePath(directoryPath, dir), dir => dir);
            return directories;
        }

        private ILookup<string, FileModel> FilesByMD5HashProcessor(string directoryPath)
        {
            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)
                                 .Select(file => CreateFileModel(file, directoryPath))
                                 .Where(fileModel => fileModel != null)
                                 .ToLookup(fileModel => fileModel.MD5Hash);
            return files;
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
                    FileReletivePath = FileSyncHelper.GetRelativePath(baseDirectory, absoluteFilePath),
                    FileSize = fileInfo.Length,
                    MD5Hash = FileSyncHelper.CalculateMD5(absoluteFilePath),
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
    }
}
