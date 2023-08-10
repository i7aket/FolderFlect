﻿using FolderFlect.Config;
using FolderFlect.Services;
using FolderFlect.Services.IServices;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace FolderFlect.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFolderFlectServices(this IServiceCollection services, AppConfig configuration)
        {
            services.AddSingleton(configuration);
            services.AddSingleton<ILogger>(provider => LogManager.GetCurrentClassLogger());
            services.AddSingleton<IFileSynchronizerService, FileSynchronizerService>();
            services.AddSingleton<IFileScannerService, FileScannerService>();
            services.AddSingleton<IFileComparerService, FileComparerService>();
            services.AddSingleton<ISchedulerService, SchedulerService>();
            services.AddSingleton<ISynchronisationManagerService, SynchronisationManagerService>();

            return services;
        }
    }
}
