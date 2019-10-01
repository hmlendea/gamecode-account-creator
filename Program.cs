using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NuciDAL.Repositories;
using NuciLog;
using NuciLog.Configuration;
using NuciLog.Core;
using NuciWeb;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using GameCodeAccountCreator.Configuration;
using GameCodeAccountCreator.DataAccess.DataObjects;
using GameCodeAccountCreator.Service;
using GameCodeAccountCreator.Service.Processors;

namespace GameCodeAccountCreator
{
    public sealed class Program
    {
        static DataSettings dataSettings;
        static DebugSettings debugSettings;
        static NuciLoggerSettings loggingSettings;

        static IServiceProvider serviceProvider;

        static void Main(string[] args)
        {
            LoadConfiguration();
            serviceProvider = CreateIOC();
            Run();
        }

        static void Run()
        {
            IWebDriver driver = serviceProvider.GetService<IWebDriver>();
            IAccountCreator accountCreator = serviceProvider.GetService<IAccountCreator>();
            ILogger logger = serviceProvider.GetService<ILogger>();

            logger.Info(Operation.StartUp, $"Application started");

            try
            {
                accountCreator.CreateAccounts();
            }
            catch (AggregateException ex)
            {
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    logger.Fatal(Operation.Unknown, OperationStatus.Failure, innerException);
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(Operation.Unknown, OperationStatus.Failure, ex);
            }
            finally
            {
                driver?.Quit();
                logger.Info(Operation.ShutDown, $"Application stopped");
            }
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
                .AddSingleton<IWebDriver>(s => SetupDriver())
                .AddSingleton<IWebProcessor, WebProcessor>()
                .AddSingleton<IGameCodeProcessor, GameCodeProcessor>()
                .AddSingleton<ISteamProcessor, SteamProcessor>()
                .AddSingleton<IRepository<SteamAccountEntity>>(s => new CsvRepository<SteamAccountEntity>(dataSettings.AccountsStorePath))
                .AddSingleton<IAccountCreator, AccountCreator>()
                .BuildServiceProvider();
        }

        static IWebDriver SetupDriver()
        {
            ChromeOptions options = new ChromeOptions();
            options.PageLoadStrategy = PageLoadStrategy.None;
            options.AddArgument("--silent");
            options.AddArgument("--no-sandbox");
			options.AddArgument("--disable-translate");
			options.AddArgument("--disable-infobars");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--start-maximized");

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;

            IWebDriver driver = new ChromeDriver(service, options, TimeSpan.FromSeconds(debugSettings.PageLoadTimeout));
            IJavaScriptExecutor scriptExecutor = (IJavaScriptExecutor)driver;
            string userAgent = (string)scriptExecutor.ExecuteScript("return navigator.userAgent;");

            if (userAgent.Contains("Headless"))
            {
                userAgent = userAgent.Replace("Headless", "");
                options.AddArgument($"--user-agent={userAgent}");

                driver.Quit();
                driver = new ChromeDriver(service, options);
            }

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(debugSettings.PageLoadTimeout);
            driver.Manage().Window.Maximize();

            return driver;
        }
    }
}
