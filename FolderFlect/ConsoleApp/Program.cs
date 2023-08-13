using FolderFlect.Config;
using FolderFlect.Extensions;
using FolderFlect.Helpers;
using FolderFlect.Services.IServices;
using Microsoft.Extensions.DependencyInjection;
using System;

public class Program
{
    private static readonly bool DebugMode = false;  

    static void Main(string[] args)
    {
        Console.WriteLine($"{DateTime.Now}: The FolderFlect has started.");

        if (DebugMode)
        {
            args = new string[]
            {
                "-source", @"C:\FolderFlect\ToReplicate",
                "-replica", @"C:\FolderFlect\Reflection",
                "-interval", "3",
                "-log", @"C:\FolderFlect\log.txt"
            };
        }

        var loadConfigurationResult = CommandLineParser.ParseArgs(args);

        if (!loadConfigurationResult.IsSuccess)
        {
            Console.WriteLine(loadConfigurationResult.Message);
            WaitForUserInput();
            return;
        }

        var configuration = loadConfigurationResult.Value;
        var serviceProvider = ConfigureDependencyInjection(configuration);

        StartSynchronizationService(serviceProvider);
    }

    private static IServiceProvider ConfigureDependencyInjection(CommandLineConfig configuration)
    {
        return new ServiceCollection()
            .AddFolderFlectServices(configuration)
            .BuildServiceProvider();
    }

    private static void StartSynchronizationService(IServiceProvider serviceProvider)
    {
        var synchronisationManagerService = serviceProvider.GetRequiredService<ISynchronisationManagerService>();
        synchronisationManagerService.StartSync();
    }

    private static void WaitForUserInput()
    {
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
