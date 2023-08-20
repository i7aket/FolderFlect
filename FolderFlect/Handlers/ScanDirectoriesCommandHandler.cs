using FolderFlect.Commands;
using FolderFlect.Models;
using FolderFlect.Services.IServices;
using FolderFlect.Utilities;
using MediatR;
using NLog;

namespace FolderFlect.Handlers;
public class ScanDirectoriesCommandHandler : IRequestHandler<ScanDirectoriesCommand, Result<MD5FileSet>>
{
    private readonly IFileScannerService _fileScannerService;
    private readonly ILogger _logger;
    private const string ErrorMessage = "Error retrieving all file paths.";

    public ScanDirectoriesCommandHandler(IFileScannerService fileScannerService, ILogger logger)
    {
        _fileScannerService = fileScannerService;
        _logger = logger;
    }

    public async Task<Result<MD5FileSet>> Handle(ScanDirectoriesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.Debug("Start RetrieveFilesGroupedByMD5AndDirectoryPaths.");
            var result = await _fileScannerService.RetrieveFilesGroupedByMD5AndDirectoryPathsAsync();
            _logger.Debug("Finished RetrieveFilesGroupedByMD5AndDirectoryPaths with success.");
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ErrorMessage);
            return Result<MD5FileSet>.Fail(ErrorMessage);
        }
    }
}
