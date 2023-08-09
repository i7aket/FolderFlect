using System;
using System.Collections.Generic;
using System.Linq;
using FolderFlect.Config;
using FolderFlect.Logging;
using FolderFlect.Models;
using FolderFlect.Services.IServices;
using FolderFlect.Utilities;

namespace FolderFlect.Services
{
    public class DirectoryComparerService : IDirectoryComparerService
    {
        private readonly ILogger _logger;

        public DirectoryComparerService(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Result<List<string>> GetDirectoriesToDelete(Dictionary<string, string> sourceDirectory, Dictionary<string, string> destDirectory)
        {
            try
            {
                var directoriesToDelete = new List<string>();

                foreach (var key in destDirectory.Keys)
                {
                    if (!sourceDirectory.ContainsKey(key))
                    {
                        directoriesToDelete.Add(key);
                    }
                }

                return Result<List<string>>.Success(directoriesToDelete);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error finding directories to delete: {ex.Message}");
                return Result<List<string>>.Fail($"Error finding directories to delete: {ex.Message}");
            }
        }

        public Result<List<string>> GetDirectoriesToCreate(Dictionary<string, string> sourceDirectory, Dictionary<string, string> destDirectory)
        {
            try
            {
                var directoriesToCreate = new List<string>();

                foreach (var key in sourceDirectory.Keys)
                {
                    if (!destDirectory.ContainsKey(key))
                    {
                        directoriesToCreate.Add(key);
                    }
                }

                return Result<List<string>>.Success(directoriesToCreate);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error finding directories to create: {ex.Message}");
                return Result<List<string>>.Fail($"Error finding directories to create: {ex.Message}");
            }
        }
    }
}
