using FolderFlect.Config;
using FolderFlect.Logging;
using FolderFlect.Models;
using FolderFlect.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

public class DirectoryScannerService : IDirectoryScannerService
{
    private readonly ILogger _logger;
    private readonly string _sourcePath;
    private readonly string _replicaPath;

    public DirectoryScannerService(ILogger logger, AppConfig appConfig)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sourcePath = appConfig.SourcePath;
        _replicaPath = appConfig.ReplicaPath;
    }

    // Scans the source directory and returns a dictionary with relative paths as keys and full paths as values.
    public Result<Dictionary<string, string>> ScanSourceDirectoryByRelativePath()
    {
        return ScanDirectories(_sourcePath);
    }

    // Scans the destination directory and returns a dictionary with relative paths as keys and full paths as values.
    public Result<Dictionary<string, string>> ScanDestinationDirectoryByRelativePath()
    {
        return ScanDirectories(_replicaPath);
    }

    // Core method to scan directories and generate a dictionary mapping from relative to full paths.
    private Result<Dictionary<string, string>> ScanDirectories(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                // If directory doesn't exist, create it.
                Directory.CreateDirectory(directoryPath);
                return Result<Dictionary<string, string>>.Success(new Dictionary<string, string>());
            }

            var directoryMap = new Dictionary<string, string>();

            // Retrieve all sub-directories recursively.
            var allDirectories = Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories);
            foreach (var dir in allDirectories)
            {
                var relativePath = PathHelper.GetRelativePath(directoryPath, dir);
                directoryMap[relativePath] = dir;
            }

            return Result<Dictionary<string, string>>.Success(directoryMap);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error scanning directories: {ex.Message}");
            return Result<Dictionary<string, string>>.Fail($"Error scanning directories: {ex.Message}");
        }
    }
}
