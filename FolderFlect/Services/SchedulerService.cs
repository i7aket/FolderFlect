using FolderFlect.Config;
using FolderFlect.Services.IServices;
using FolderFlect.Utilities;
using NLog;
using Timer = System.Timers.Timer;

namespace FolderFlect.Services;

/// <summary>
/// Provides a service for scheduling and executing tasks at defined intervals.
/// </summary>
public class SchedulerService : ISchedulerService
{
    #region Fields and Constructor

    private const int MillisecondsPerSecond = 1000;

    private Timer _timer;
    private readonly ILogger _logger;
    private readonly int _syncIntervalInMilliseconds;

    // Lock to ensure synchronization tasks don't overlap
    private readonly object _syncLock = new object();
    private bool _isTaskRunning = false;

    public delegate Task TaskToRunHandlerAsync();
    public event TaskToRunHandlerAsync OnExecuteAsync;

    public SchedulerService(ILogger logger, CommandLineConfig appConfig)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (appConfig == null) throw new ArgumentNullException(nameof(appConfig));
        if (appConfig.SyncInterval <= 0)
            throw new ArgumentException("SyncInterval must be a positive value.", nameof(appConfig.SyncInterval));

        _syncIntervalInMilliseconds = appConfig.SyncInterval * MillisecondsPerSecond;
        _timer = new Timer(_syncIntervalInMilliseconds);
        _timer.Elapsed += async (sender, e) => await ExecuteScheduledTaskAsync();

        _logger.Debug($"SchedulerService constructed with sync interval: {TimeHelper.GetInterval(appConfig.SyncInterval)}.");
    }

    #endregion

    private async Task ExecuteScheduledTaskAsync()
    {
        _logger.Debug("Attempting to execute scheduled synchronization...");

        lock (_syncLock)
        {
            if (_isTaskRunning)
            {
                _logger.Debug("The previous synchronization is still in progress.");
                return;
            }
            _isTaskRunning = true;
        }

        try
        {
            if (OnExecuteAsync != null) await OnExecuteAsync.Invoke();
        }
        catch (Exception ex)
        {
            _logger.Error($"Error executing synchronization: {ex.Message}");
        }
        finally
        {
            _isTaskRunning = false;
        }

        _logger.Debug("Finished executing synchronization.");
    }

    public void Start()
    {
        _timer.Start();
        _logger.Debug("Scheduler timer started.");
    }

    public void Stop()
    {
        _timer.Stop();
        _logger.Debug("Scheduler timer stopped.");
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}

