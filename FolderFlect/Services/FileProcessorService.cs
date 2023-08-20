using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FolderFlect.Models;
using FolderFlect.Services.IServices;
using FolderFlect.Utilities;
using NLog;

namespace FolderFlect.Services
{
    /// <summary>
    /// Service responsible for file operations, such as moving, copying, deleting, and creating directories.
    /// </summary>
    public class FileProcessorService : IFileProcessorService
    {
        private readonly ILogger _logger;

        public FileProcessorService(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Asynchronously moves files from source to destination paths.
        /// </summary>
        /// <param name="absolutePathsToMove">List of tuples containing source and destination paths.</param>
        /// <returns>Result of the file move operation.</returns>
        public async Task<FileProcessorResult> MoveFilesAsync(List<(string SourcePath, string DestinationPath)> absolutePathsToMove)
        {
            _logger.Debug("Attempting to move files...");

            var result = new FileProcessorResult();

            foreach (var (sourcePath, destPath) in absolutePathsToMove)
            {
                var (conditionMet, errorMessage) = CheckFileConditions(sourcePath, destPath);
                if (!conditionMet)
                {
                    result.FailedToProcessTuples.Add((sourcePath, destPath, errorMessage));
                    continue;
                }

                await FileSyncHelper.SetFileAsWritableAsync(sourcePath);

                try
                {
                    File.Move(sourcePath, destPath);
                    result.SuccessfullyProcessedTuples.Add((sourcePath, destPath));
                }
                catch (Exception ex)
                {
                    result.FailedToProcessTuples.Add((sourcePath, destPath, ex.Message));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously copies files from source to destination paths.
        /// </summary>
        /// <param name="absolutePathsToCopy">List of tuples containing source and destination paths.</param>
        /// <returns>Result of the file copy operation.</returns>
        public async Task<FileProcessorResult> CopyFilesAsync(List<(string SourcePath, string DestinationPath)> absolutePathsToCopy)
        {
            _logger.Debug("Attempting to copy files...");

            var result = new FileProcessorResult();

            foreach (var (sourcePath, destPath) in absolutePathsToCopy)
            {
                var (conditionMet, errorMessage) = CheckFileConditions(sourcePath, destPath);
                if (!conditionMet)
                {
                    result.FailedToProcessTuples.Add((sourcePath, destPath, errorMessage));
                    continue;
                }

                try
                {
                    await CopyFileAsync(sourcePath, destPath);
                    result.SuccessfullyProcessedTuples.Add((sourcePath, destPath));
                }
                catch (Exception ex)
                {
                    result.FailedToProcessTuples.Add((sourcePath, destPath, ex.Message));
                }
            }

            return result;
        }

        private async Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            using (var destinationStream = new FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }
        }

        /// <summary>
        /// Asynchronously deletes files specified in the provided paths.
        /// </summary>
        /// <param name="absolutePathsToDelete">List of file paths to delete.</param>
        /// <returns>Result of the file delete operation.</returns>
        public async Task<FileProcessorResult> DeleteFilesAsync(List<string> absolutePathsToDelete)
        {
            _logger.Debug("Attempting to delete files...");

            var result = new FileProcessorResult();

            foreach (var path in absolutePathsToDelete)
            {
                if (!File.Exists(path))
                {
                    result.FailedToProcess.Add((path, "File does not exist."));
                    continue;
                }

                await FileSyncHelper.SetFileAsWritableAsync(path);

                try
                {
                    File.Delete(path);
                    result.SuccessfullyProcessed.Add(path);
                }
                catch (Exception ex)
                {
                    result.FailedToProcess.Add((path, ex.Message));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously deletes directories specified in the provided paths.
        /// </summary>
        /// <param name="absolutePathsToDelete">List of directory paths to delete.</param>
        /// <returns>Result of the directory delete operation.</returns>
        public async Task<FileProcessorResult> DeleteDirectoriesAsync(List<string> absolutePathsToDelete)
        {
            _logger.Debug("Attempting to delete directories...");

            var result = new FileProcessorResult();

            foreach (var path in absolutePathsToDelete)
            {
                if (!Directory.Exists(path))
                {
                    result.FailedToProcess.Add((path, "Directory does not exist."));
                    continue;
                }

                try
                {
                    Directory.Delete(path, recursive: true);
                    result.SuccessfullyProcessed.Add(path);
                }
                catch (Exception ex)
                {
                    result.FailedToProcess.Add((path, ex.Message));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously creates directories specified in the provided paths.
        /// </summary>
        /// <param name="absolutePathsToCreate">List of directory paths to create.</param>
        /// <returns>Result of the directory creation operation.</returns>
        public async Task<FileProcessorResult> CreateDirectoriesAsync(List<string> absolutePathsToCreate)
        {
            _logger.Debug("Attempting to create directories...");

            var result = new FileProcessorResult();

            foreach (var path in absolutePathsToCreate)
            {
                if (Directory.Exists(path))
                {
                    result.FailedToProcess.Add((path, "Directory already exists."));
                    continue;
                }

                try
                {
                    Directory.CreateDirectory(path);
                    result.SuccessfullyProcessed.Add(path);
                }
                catch (Exception ex)
                {
                    result.FailedToProcess.Add((path, ex.Message));
                }
            }

            return result;
        }

        /// <summary>
        /// Checks conditions for file operations.
        /// </summary>
        /// <param name="sourcePath">The source file path.</param>
        /// <param name="destinationPath">The destination file path (optional).</param>
        /// <returns>A tuple indicating whether conditions are met and an error message, if any.</returns>
        private (bool ConditionMet, string ErrorMessage) CheckFileConditions(string sourcePath, string destinationPath = null)
        {
            if (!File.Exists(sourcePath))
            {
                return (false, "Source file does not exist.");
            }

            if (destinationPath != null)
            {
                if (File.Exists(destinationPath))
                {
                    return (false, "Destination file already exists.");
                }

                var destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(destinationDirectory))
                {
                    return (false, "Destination directory does not exist.");
                }
            }

            return (true, string.Empty);
        }
    }
}
