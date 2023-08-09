using FolderFlect.Config;
using FolderFlect.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class SchedulerService : ISchedulerService
{
    private CancellationTokenSource _cancellationTokenSource;
    private readonly ILogger _logger;
    private readonly int _syncIntervalInMilliseconds;

    public delegate void TaskToRunHandler();
    public event TaskToRunHandler OnExecute;

    public SchedulerService(ILogger logger, AppConfig appConfig)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _syncIntervalInMilliseconds = appConfig.SyncInterval * 1000;
    }

    public void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        Task.Run(async () =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    OnExecute?.Invoke();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error executing scheduled task: {ex.Message}");
                }

                try
                {
                    await Task.Delay(_syncIntervalInMilliseconds, _cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    // Expected exception when cancellation token is triggered
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error during task delay: {ex.Message}");
                }
            }
        });
    }

    public void Stop()
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = null;
        }
    }
}
