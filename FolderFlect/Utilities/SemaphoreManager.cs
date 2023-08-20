namespace FolderFlect.Utilities;

public static class SemaphoreManager
{
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10);  

    public static async Task<T> WithSemaphore<T>(Func<Task<T>> taskGenerator)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await taskGenerator();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public static async Task WithSemaphore(Func<Task> taskGenerator)
    {
        await _semaphore.WaitAsync();
        try
        {
            await taskGenerator();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}


