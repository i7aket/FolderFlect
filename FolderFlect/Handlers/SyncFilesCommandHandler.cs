using FolderFlect.Commands;
using FolderFlect.Services.IServices;
using FolderFlect.Utilities;
using MediatR;
using NLog;

namespace FolderFlect.Handlers;
public class SyncFilesCommandHandler : IRequestHandler<SyncFilesCommand, Result>
{
    private readonly IFileSynchronizerService _fileSynchronizerService;
    private const string ErrorMessage = "Error occurred during file synchronization.";
    private readonly ILogger _logger;


    public SyncFilesCommandHandler(IFileSynchronizerService fileSynchronizerService, ILogger logger)
    {
        _fileSynchronizerService = fileSynchronizerService ?? throw new ArgumentNullException(nameof(fileSynchronizerService));
        _logger = logger;
    }

    public async Task<Result> Handle(SyncFilesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.Debug("Starting SyncFilesByMD5...");
            var result = await _fileSynchronizerService.SyncFilesByMD5(request.FilesToSyncSet);
            _logger.Debug("Finished SyncFilesByMD5...");
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, ErrorMessage);
            return Result.Fail($"{ErrorMessage} {ex.Message}");
        }
        
    }
}
