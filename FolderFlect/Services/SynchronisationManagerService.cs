using FolderFlect.Commands;
using FolderFlect.Config;
using FolderFlect.Services.IServices;
using FolderFlect.Utilities;
using MediatR;
using NLog;

namespace FolderFlect.Services;

/// <summary>
/// Manages the synchronization of files, utilizing other services to scan, compare, and synchronize.
/// </summary>
public class SynchronisationManagerService : ISynchronisationManagerService
{
    #region Fields and Constructor

    private readonly CommandLineConfig _config;
    private readonly ILogger _logger;
    private readonly ISchedulerService _scheduler;
    private readonly IMediator _mediator;

    public SynchronisationManagerService(
        CommandLineConfig config,
        ILogger logger,
        ISchedulerService scheduler,
        IMediator mediator
        )
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        Helpers.NLogConfiguraion.ConfigureNLog(_config); 

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _scheduler.OnExecuteAsync += Sync; 
        _mediator = mediator;

        _logger.Debug("Service initialized.");
    }

    #endregion

    /// <summary>
    /// Initiates the synchronization process.
    /// </summary>
    public void StartSync()
    {
        _logger.Debug("Starting synchronization...");

        _scheduler.Start();
        _logger.Info($"{_config}{Environment.NewLine}Press any key to stop...");
        Console.ReadLine();
        _scheduler.Stop();

        _logger.Debug("Synchronization stopped.");
    }

    /// <summary>
    /// Executes the full synchronization process: scanning, comparing, and syncing files.
    /// </summary>
    private async Task Sync()
    {
        _logger.Debug("Starting synchronization...");

        var scanResult = await _mediator.Send(new ScanDirectoriesCommand());
        if (!ProcessResult(scanResult, "Scanning Files")) return;

        var filesToSyncResult = await _mediator.Send(new CompareFilesCommand(scanResult.Value));
        if (!ProcessResult(filesToSyncResult, "Comparing Files")) return;

        var syncResult = await _mediator.Send(new SyncFilesCommand(filesToSyncResult.Value));
        if (!ProcessResult(syncResult, "Syncing Files")) return;

        _logger.Debug("Synchronization finished.");
    }


    /// <summary>
    /// Processes the result of an operation, logging any errors encountered.
    /// </summary>
    /// <param name="result">The result of the operation.</param>
    /// <param name="operationName">The name of the operation for logging purposes.</param>
    private bool ProcessResult(IResult result, string operationName)
    {
        if (!result.IsSuccess)
        {
            _logger.Error(result.Message);
            _logger.Debug($"PerformSynchronisation terminated early due to {operationName} failure.");
            return false; 
        }
        return true; 
    }
}

