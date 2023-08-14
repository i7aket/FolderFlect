using FolderFlect.Config;
using FolderFlect.Extensions;
using FolderFlect.Helpers;
using FolderFlect.Services.IServices;
using Microsoft.Extensions.DependencyInjection;
using PowerArgs;
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
                "-interval", "60",
                "-log", @"C:\FolderFlect\log.txt"
            };
        }

        try
        {
            var configuration = Args.Parse<CommandLineConfig>(args);
            var serviceProvider = ConfigureDependencyInjection(configuration);
            StartSynchronizationService(serviceProvider);
        }
        catch (ArgException ex)
        {
            Console.WriteLine($"Argument error: {ex.Message}");
            Console.WriteLine(ArgUsage.GenerateUsageFromTemplate<CommandLineConfig>());
            Console.ReadKey();
            Environment.Exit(1);
        }
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
}
