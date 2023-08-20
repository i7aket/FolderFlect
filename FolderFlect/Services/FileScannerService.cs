using FolderFlect.Config;
using FolderFlect.Extensions;
using FolderFlect.Models;
using FolderFlect.Services.IServices;
using FolderFlect.Utilities;
using NLog;

namespace FolderFlect.Services;
/// <summary>
/// Service responsible for scanning files.
/// </summary>
public class FileScannerService : IFileScannerService
{
    #region Fields and Constructor

    private readonly ILogger _logger;
    private readonly (string Path, string Name) _sourcePathInfo;
    private readonly (string Path, string Name) _replicaPathInfo;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileScannerService"/> class.
    /// </summary>
    /// <param name="logger">Instance of a logger.</param>
    /// <param name="appConfig">Application configuration.</param>
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
    /// Asynchronously retrieves groups of files by their MD5 hash and directory paths.
    /// </summary>
    /// <returns>A task representing the asynchronous operation with a result of type <see cref="Result{MD5FileSet}"/>.</returns>
    public async Task<Result<MD5FileSet>> RetrieveFilesGroupedByMD5AndDirectoryPathsAsync()
    {

        var sourceFilesByMD5Hash = FilesByMD5HashProcessorAsync(_sourcePathInfo);
        var destForFilesByMD5Hash = FilesByMD5HashProcessorAsync(_replicaPathInfo);

        await Task.WhenAll(sourceFilesByMD5Hash, destForFilesByMD5Hash);

        var sourceDirectoriesByRelativePath = DirectoriesByRelativePathProcessorAsync(_sourcePathInfo);
        var destDirectoriesByRelativePath = DirectoriesByRelativePathProcessorAsync(_replicaPathInfo);

        await Task.WhenAll(sourceDirectoriesByRelativePath, destDirectoriesByRelativePath);

        var fileSet = new MD5FileSet(
            sourceFilesByMD5Hash.Result,
            destForFilesByMD5Hash.Result,
            sourceDirectoriesByRelativePath.Result,
            destDirectoriesByRelativePath.Result
        );

        _logger.Debug("Finished RetrieveFilesGroupedByMD5AndDirectoryPaths with success.");
        return Result<MD5FileSet>.Success(fileSet);

    }

    private async Task<Dictionary<string, string>> DirectoriesByRelativePathProcessorAsync((string Path, string Name) directoryInfo)
    {
        _logger.LogPathInfo("Starting scanning for", directoryInfo);
        await FileSyncHelper.EnsureDirectoryExistsAsync(directoryInfo.Path);

        var directories = Directory.GetDirectories(directoryInfo.Path, "*", SearchOption.AllDirectories)
                                   .ToDictionary(dir => FileSyncHelper.GetRelativePath(directoryInfo.Path, dir), dir => dir);
        return directories;
    }

    private async Task<ILookup<string, FileModel>> FilesByMD5HashProcessorAsync((string Path, string Name) directoryInfo)
    {
        _logger.LogPathInfo("Starting scanning for", directoryInfo);
        await FileSyncHelper.EnsureDirectoryExistsAsync(directoryInfo.Path);

        var allFilesInDirectory = Directory.GetFiles(directoryInfo.Path, "*", SearchOption.AllDirectories);

        var fileTasks = allFilesInDirectory.Select(file => CreateFileModelAsync(file, directoryInfo.Path))
                                           .ToList();

        var fileModels = await Task.WhenAll(fileTasks);

        var files = fileModels.Where(fileModel => fileModel != null)
                              .ToLookup(fileModel => fileModel?.MD5Hash);

        return files;
    }


    private async Task<FileModel?> CreateFileModelAsync(string absoluteFilePath, string baseDirectory)
    {
        return await SemaphoreManager.WithSemaphore(async () =>
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
                    MD5Hash = await FileSyncHelper.CalculateMD5Async(absoluteFilePath),
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
        });
    }
}

