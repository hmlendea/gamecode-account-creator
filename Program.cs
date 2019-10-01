using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NuciDAL.Repositories;
using NuciLog;
using NuciLog.Configuration;
using NuciLog.Core;

using GameCodeAccountCreator.Configuration;
using GameCodeAccountCreator.DataAccess.DataObjects;
using GameCodeAccountCreator.Service;

namespace GameCodeAccountCreator
{
    public sealed class Program
    {
        static DataSettings dataSettings;
        static DebugSettings debugSettings;
        static NuciLoggerSettings loggingSettings;

        static IServiceProvider serviceProvider;
        static ILogger logger;

        static void Main(string[] args)
        {
            LoadConfiguration();
            serviceProvider = CreateIOC();

            logger = serviceProvider.GetService<ILogger>();
            logger.SetSourceContext<Program>();

            logger.Info(Operation.StartUp, $"Application started");
            Run();
            logger.Info(Operation.ShutDown, $"Application stopped");
        }
        
        static IConfiguration LoadConfiguration()
        {
            dataSettings = new DataSettings();
            debugSettings = new DebugSettings();
            loggingSettings = new NuciLoggerSettings();
            
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            config.Bind(nameof(DataSettings), dataSettings);
            config.Bind(nameof(DebugSettings), debugSettings);
            config.Bind(nameof(NuciLoggerSettings), loggingSettings);

            return config;
        }

        static IServiceProvider CreateIOC()
        {
            return new ServiceCollection()
                .AddSingleton(dataSettings)
                .AddSingleton(debugSettings)
                .AddSingleton(loggingSettings)
                .AddSingleton<ILogger, NuciLogger>()
                .AddSingleton<IRepository<SteamAccountEntity>>(s => new CsvRepository<SteamAccountEntity>(dataSettings.AccountsStorePath))
                .AddSingleton<IAccountCreator, AccountCreator>()
                .BuildServiceProvider();
        }

        static void Run()
        {
            IAccountCreator accountCreator = serviceProvider.GetService<IAccountCreator>();
            accountCreator.CreateAccounts();
        }
    }
}
