using FolderFlect.Config;
using FolderFlect.Extensions;
using FolderFlect.Services.IServices;
using Microsoft.Extensions.DependencyInjection;
using PowerArgs;

namespace FolderFlect;

public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine($"{DateTime.Now}: The FolderFlect has started.");

        if (DebugMode.IsEnabled) args = DebugMode.GetArgs();

        try
        {
            InitializeAndStartSync(args);
        }
        catch (ArgException ex)
        {
            Console.WriteLine($"Argument error: {ex.Message}");
            Console.WriteLine(ArgUsage.GenerateUsageFromTemplate<CommandLineConfig>());
            Console.ReadKey();
            Environment.Exit(1);
        }
    }

    private static void InitializeAndStartSync(string[] args)
    {
        var configuration = Args.Parse<CommandLineConfig>(args);
        var serviceProvider = new ServiceCollection()
                                .AddFolderFlectServices(configuration)
                                .BuildServiceProvider();

        var synchronisationManagerService = serviceProvider.GetRequiredService<ISynchronisationManagerService>();
        synchronisationManagerService.StartSync();
    }
}

