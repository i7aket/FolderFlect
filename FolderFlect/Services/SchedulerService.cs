using FolderFlect.Config;
using NLog;
using System;
using System.Timers;
using Timer = System.Timers.Timer;

public class SchedulerService : ISchedulerService
{
    private Timer _timer;
    private readonly ILogger _logger;
    private readonly int _syncIntervalInMilliseconds;

    public delegate void TaskToRunHandler();
    public event TaskToRunHandler OnExecute;

    public SchedulerService(ILogger logger, AppConfig appConfig)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _syncIntervalInMilliseconds = appConfig.SyncInterval * 1000;

        _timer = new Timer(_syncIntervalInMilliseconds);
        _timer.Elapsed += (sender, e) => ExecuteScheduledTask();
    }

    private void ExecuteScheduledTask()
    {
        try
        {
            OnExecute?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.Error($"Error executing scheduled task: {ex.Message}");
        }
    }

    public void Start()
    {
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }
}
