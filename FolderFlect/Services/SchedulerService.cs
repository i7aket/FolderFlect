using FolderFlect.Config;
using FolderFlect.Utilities;
using NLog;
using System;
using System.Timers;
using Timer = System.Timers.Timer;

public class SchedulerService : ISchedulerService
{
    #region Fields and Constructor

    private const int MillisecondsPerSecond = 1000;

    private Timer _timer;
    private readonly ILogger _logger;
    private readonly int _syncIntervalInMilliseconds;
    private readonly object _syncLock = new object();
    private bool _isTaskRunning = false;

    public delegate void TaskToRunHandler();
    public event TaskToRunHandler OnExecute;

    public SchedulerService(ILogger logger, AppConfig appConfig)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _syncIntervalInMilliseconds = appConfig.SyncInterval * MillisecondsPerSecond;

        _timer = new Timer(_syncIntervalInMilliseconds);
        _timer.Elapsed += (sender, e) => ExecuteScheduledTask();

        _logger.Debug($"SchedulerService constructed with sync interval: {TimeHelper.GetInterval(appConfig.SyncInterval)}.");

    }
    #endregion
    private void ExecuteScheduledTask()
    {
        _logger.Debug("Attempting to execute scheduled task...");

        lock (_syncLock)
        {
            if (_isTaskRunning)
            {
                _logger.Debug("The previous synchronization is still in progress.");
                return;
            }

            _isTaskRunning = true;

            try
            {
                OnExecute?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error executing scheduled task: {ex.Message}");
            }
            finally
            {
                _isTaskRunning = false;
            }
        }

        _logger.Debug("Finished executing scheduled task.");

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
