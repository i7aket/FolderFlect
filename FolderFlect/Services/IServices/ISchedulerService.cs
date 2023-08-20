namespace FolderFlect.Services.IServices;

public interface ISchedulerService : IDisposable
{
    event SchedulerService.TaskToRunHandlerAsync OnExecuteAsync;
    void Start();
    void Stop();
}