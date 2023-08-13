using FolderFlect.Config;
using FolderFlect.Utilities;
using NLog;
using System;
using System.Timers;
using Timer = System.Timers.Timer;

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

    // Delegate and event to be invoked when a task is to be executed
    public delegate void TaskToRunHandler();
    public event TaskToRunHandler OnExecute;

    public SchedulerService(ILogger logger, CommandLineConfig appConfig)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (appConfig == null)
            throw new ArgumentNullException(nameof(appConfig));

        if (appConfig.SyncInterval <= 0)
            throw new ArgumentException("SyncInterval must be a positive value.", nameof(appConfig.SyncInterval));

        _syncIntervalInMilliseconds = appConfig.SyncInterval * MillisecondsPerSecond;

        _timer = new Timer(_syncIntervalInMilliseconds);
        _timer.Elapsed += (sender, e) => ExecuteScheduledTask();

        _logger.Debug($"SchedulerService constructed with sync interval: {TimeHelper.GetInterval(appConfig.SyncInterval)}.");
    }

    #endregion

    /// <summary>
    /// Executes the scheduled task. Ensures only one instance of the task runs at any given time.
    /// </summary>
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

    /// <summary>
    /// Starts the scheduler timer.
    /// </summary>
    public void Start()
    {
        _timer.Start();
        _logger.Debug("Scheduler timer started.");
    }

    /// <summary>
    /// Stops the scheduler timer.
    /// </summary>
    public void Stop()
    {
        _timer.Stop();
        _logger.Debug("Scheduler timer stopped.");
    }

    /// <summary>
    /// Disposes of the timer resources.
    /// </summary>
    public void Dispose()
    {
        _timer.Dispose();
    }
}
