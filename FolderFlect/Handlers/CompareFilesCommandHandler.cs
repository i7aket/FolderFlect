using FolderFlect.Commands;
using FolderFlect.Models;
using FolderFlect.Services.IServices;
using FolderFlect.Utilities;
using MediatR;
using NLog;

namespace FolderFlect.Handlers;
public class CompareFilesCommandHandler : IRequestHandler<CompareFilesCommand, Result<FilesToSyncSetByMD5>>
{
    private readonly IFileComparerService _fileComparerService;
    private readonly ILogger _logger;
    private const string ErrorMessage = "Error during files synchronization:";


    public CompareFilesCommandHandler(IFileComparerService fileComparerService, ILogger logger)
    {
        _fileComparerService = fileComparerService;
        _logger = logger;
    }

    public async Task<Result<FilesToSyncSetByMD5>> Handle(CompareFilesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.Debug("Starting GetFilesToSyncGroupedByMD5AndDirectoryPaths");
            var result = await _fileComparerService.GetFilesToSyncGroupedByMD5AndDirectoryPathsAsync(request.FileSet);
            _logger.Debug("Finished GetFilesToSyncGroupedByMD5AndDirectoryPaths with success.");
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error($"{ErrorMessage} {ex.Message}");
            return Result<FilesToSyncSetByMD5>.Fail($"{ErrorMessage} {ex.Message}");
        }
    }
}
