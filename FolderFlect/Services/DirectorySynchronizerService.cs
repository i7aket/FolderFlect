using System;
using System.Collections.Generic;
using System.IO;
using FolderFlect.Config;
using FolderFlect.Logging;
using FolderFlect.Models;
using FolderFlect.Services.IServices;
using FolderFlect.Utilities;

namespace FolderFlect.Services
{
    public class DirectorySynchronizerService : IDirectorySynchronizerService
    {
        private readonly ILogger _logger;
        private readonly string _replicaPath;


        public DirectorySynchronizerService(ILogger logger, AppConfig appConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _replicaPath = appConfig.ReplicaPath;

        }

        public Result DeleteDirectories(List<string> directoriesToDelete)
        {
            try
            {
                if (directoriesToDelete == null)
                    throw new ArgumentNullException(nameof(directoriesToDelete));

                if (string.IsNullOrWhiteSpace(_replicaPath))
                    throw new ArgumentNullException(nameof(_replicaPath));

                foreach (var relativeDirectoryPath in directoriesToDelete)
                {
                    var fullPathToDelete = Path.Combine(_replicaPath, relativeDirectoryPath);

                    if (Directory.Exists(fullPathToDelete))
                    {
                        Directory.Delete(fullPathToDelete, recursive: true);

                        _logger.Log($"Deleted directory: {fullPathToDelete}");
                    }
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.Log($"Error while deleting directories: {ex.Message}");
                return Result.Fail($"Error while deleting directories: {ex.Message}");
            }
        }

        public Result CreateDirectories(List<string> directoriesToCreate)
        {
            try
            {
                if (directoriesToCreate == null)
                    throw new ArgumentNullException(nameof(directoriesToCreate));

                if (string.IsNullOrWhiteSpace(_replicaPath))
                    throw new ArgumentNullException(nameof(_replicaPath));

                foreach (var relativeDirectoryPath in directoriesToCreate)
                {
                    var fullPathToCreate = Path.Combine(_replicaPath, relativeDirectoryPath);

                    if (!Directory.Exists(fullPathToCreate))
                    {
                        Directory.CreateDirectory(fullPathToCreate);
                        _logger.Log($"Created directory: {fullPathToCreate}");
                    }
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.Log($"Error while creating directories: {ex.Message}");
                return Result.Fail($"Error while creating directories: {ex.Message}");
            }
        }
    }
}
