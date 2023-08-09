using FolderFlect.Config;
using FolderFlect.Helpers;
using FolderFlect.Services;

public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine($"{DateTime.Now}: The program has started.");

        var loadConfigurationResult = ConfigurationLoader.LoadConfiguration(args);

        if (!loadConfigurationResult.IsSuccess)
        {
            Console.WriteLine(loadConfigurationResult.Message);
            Console.ReadLine();
            return;
        }

        var configuration = loadConfigurationResult.Value;

        //var configuration = new AppConfig(@"C:\FolderFlect\ToReplicate", @"C:\FolderFlect\Reflection", 1, @"C:\FolderFlect\log.txt");

        var synchronisationManagerService = new SynchronisationManagerService(configuration);

        synchronisationManagerService.RunFolderSynchronisation(); 
    }
}