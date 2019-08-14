using System;
using System.Collections.Generic;
using System.Linq;

using NuciDAL.Repositories;
using NuciLog.Core;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using GameCodeAccountCreator.Configuration;
using GameCodeAccountCreator.DataAccess.DataObjects;
using GameCodeAccountCreator.Logging;
using GameCodeAccountCreator.Service.Mapping;
using GameCodeAccountCreator.Service.Models;
using GameCodeAccountCreator.Service.Processors;

namespace GameCodeAccountCreator.Service
{
    public sealed class AccountCreator : IAccountCreator
    {
        readonly IRepository<SteamAccountEntity> accountsRepository;
        readonly DebugSettings debugSettings;
        readonly ILogger logger;

        public AccountCreator(
            IRepository<SteamAccountEntity> accountsRepository,
            DebugSettings debugSettings,
            ILogger logger)
        {
            this.accountsRepository = accountsRepository;
            this.debugSettings = debugSettings;
            this.logger = logger;
        }

        public void CreateAccounts()
        {
            IEnumerable<SteamAccount> accounts = accountsRepository.GetAll().ToServiceModels().ToList();

            foreach (SteamAccount account in accounts)
            {
                CreateAccount(account);

                accountsRepository.Remove(account.Id);
                accountsRepository.ApplyChanges();
            }
        }

        void CreateAccount(SteamAccount account)
        {
            IWebDriver driver = SetupDriver();

            try
            {
                LogInToSteam(driver, account);
                RegisterOnGameCode(driver, account);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                driver.Quit();
            }
        }

        void LogInToSteam(IWebDriver driver, SteamAccount account)
        {
            logger.Info(MyOperation.SteamLogIn, OperationStatus.Started, new LogInfo(MyLogInfoKey.Username, account.Username));

            try
            {
                ISteamProcessor steamProcessor = new SteamProcessor(driver, account);
                steamProcessor.LogIn();
                steamProcessor.Dispose();

                logger.Debug(MyOperation.SteamLogIn, OperationStatus.Success, new LogInfo(MyLogInfoKey.Username, account.Username));
            }
            catch (Exception ex)
            {
                logger.Error(MyOperation.SteamLogIn, OperationStatus.Failure, ex, new LogInfo(MyLogInfoKey.Username, account.Username));
                throw;
            }
        }

        void RegisterOnGameCode(IWebDriver driver, SteamAccount account)
        {
            logger.Info(MyOperation.GameCodeRegistration, OperationStatus.Started, new LogInfo(MyLogInfoKey.Username, account.Username));

            try
            {
                IGameCodeProcessor gameCodeProcessor = new GameCodeProcessor(driver, account);
                gameCodeProcessor.Register();
                gameCodeProcessor.LinkSteamAccount();
                gameCodeProcessor.Dispose();

                logger.Debug(MyOperation.GameCodeRegistration, OperationStatus.Success, new LogInfo(MyLogInfoKey.Username, account.Username));
            }
            catch (Exception ex)
            {
                logger.Error(MyOperation.GameCodeRegistration, OperationStatus.Failure, ex.StackTrace, ex, new LogInfo(MyLogInfoKey.Username, account.Username));
                throw;
            }
        }

        IWebDriver SetupDriver()
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
