public interface ISchedulerService
{
    event SchedulerService.TaskToRunHandler OnExecute;

    void Start();
    void Stop();
    void Dispose();

}