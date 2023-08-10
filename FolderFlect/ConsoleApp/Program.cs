using FolderFlect.Config;
using FolderFlect.Extensions;
using FolderFlect.Helpers;
using FolderFlect.Services.IServices;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;

public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine($"{DateTime.Now}: Folder Reflect has started.");

        var loadConfigurationResult = ConfigurationLoader.LoadConfiguration(args);

        if (!loadConfigurationResult.IsSuccess)
        {
            Console.WriteLine(loadConfigurationResult.Message);
            Console.ReadLine();
            return;
        }

        var configuration = loadConfigurationResult.Value;

        var serviceProvider = new ServiceCollection()
            .AddFolderFlectServices(configuration)
            .BuildServiceProvider();

        var synchronisationManagerService = serviceProvider.GetRequiredService<ISynchronisationManagerService>();
        synchronisationManagerService.RunFolderSynchronisation();
    }
}
