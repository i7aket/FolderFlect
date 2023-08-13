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
using FolderFlect.Extensions;

namespace FolderFlect.Services
{
    /// <summary>
    /// Service responsible for scanning and grouping files from source and replica directories based on their MD5 hashes.
    /// It retrieves structured representations of files and directories, groups files by their MD5 hashes, 
    /// and organizes directories by their relative paths.
    /// </summary>
    public class FileScannerService : IFileScannerService
    {
        #region Fields and Constructor

        private readonly ILogger _logger;
        private readonly (string Path, string Name) _sourcePathInfo;
        private readonly (string Path, string Name) _replicaPathInfo;

        public FileScannerService(ILogger logger, CommandLineConfig appConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sourcePathInfo = appConfig.SourcePathInfo;
            _replicaPathInfo = appConfig.ReplicaPathInfo;

            _logger.LogPathInfo("Initialized with", _sourcePathInfo);
            _logger.LogPathInfo("Initialized with", _replicaPathInfo);
        }

        #endregion

        /// <summary>
        /// Retrieves a structured representation of files and directories from the source and destination paths.
        /// The files are grouped by their MD5 hashes and the directories by their relative paths.
        ///
        /// The resulting MD5FileSet contains:
        /// - SourceFiles: An ILookup of files from the source path, grouped by their MD5 hash.
        /// - DestinationFiles: An ILookup of files from the destination path, grouped by their MD5 hash.
        /// - SourceDirectories: A Dictionary representing directories from the source path with their relative paths as keys.
        /// - DestinationDirectories: A Dictionary representing directories from the destination path with their relative paths as keys.
        ///
        /// </summary>
        /// <returns>Result containing MD5FileSet on success or an error message on failure.</returns>

        public Result<MD5FileSet> RetrieveFilesGroupedByMD5AndDirectoryPaths()
        {
            try
            {
                var sourceFilesByMD5Hash = FilesByMD5HashProcessor(_sourcePathInfo);
                var destForFilesByMD5Hash = FilesByMD5HashProcessor(_replicaPathInfo);

                var sourceDirectoriesByRelativePath = DirectoriesByRelativePathProcessor(_sourcePathInfo);
                var destDirectoriesByRelativePath = DirectoriesByRelativePathProcessor(_replicaPathInfo);

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

        /// <summary>
        /// Processes a given directory to retrieve all its sub-directories,
        /// mapped by their relative paths.
        /// </summary>
        private Dictionary<string, string> DirectoriesByRelativePathProcessor((string Path, string Name) directoryInfo)
        {
            _logger.LogPathInfo("Starting scanning for", directoryInfo);
            FileSyncHelper.EnsureDirectoryExists(directoryInfo.Path);

            var directories = Directory.GetDirectories(directoryInfo.Path, "*", SearchOption.AllDirectories)
                                       .ToDictionary(dir => FileSyncHelper.GetRelativePath(directoryInfo.Path, dir), dir => dir);
            return directories;
        }

        /// <summary>
        /// Processes a given directory to retrieve all its files,
        /// and groups them by their MD5 hash values using ILookup.
        /// ILookup allows efficient lookup and preserves the order of files.
        /// The use of ILookup ensures that files with identical MD5 hash values
        /// are all retained and can be easily retrieved.
        /// </summary>
        private ILookup<string, FileModel> FilesByMD5HashProcessor((string Path, string Name) directoryInfo)
        {
            _logger.LogPathInfo("Starting scanning for", directoryInfo);
            FileSyncHelper.EnsureDirectoryExists(directoryInfo.Path);

            var files = Directory.GetFiles(directoryInfo.Path, "*", SearchOption.AllDirectories)
                                 .Select(file => CreateFileModel(file, directoryInfo.Path))
                                 .Where(fileModel => fileModel != null)
                                 .ToLookup(fileModel => fileModel.MD5Hash);
            return files;
        }


        /// <summary>
        /// Constructs a FileModel from the given absolute file path and its base directory.
        /// </summary>
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
