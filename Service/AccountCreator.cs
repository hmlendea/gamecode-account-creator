using System;
using System.Collections.Generic;
using System.Linq;

using NuciDAL.Repositories;
using NuciLog.Core;

using OpenQA.Selenium;

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
        readonly IGameCodeProcessor gameCodeProcessor;
        readonly ISteamProcessor steamProcessor;
        readonly IRepository<SteamAccountEntity> accountsRepository;
        readonly DebugSettings debugSettings;
        readonly IWebDriver webDriver;
        readonly ILogger logger;

        public AccountCreator(
            IGameCodeProcessor gameCodeProcessor,
            ISteamProcessor steamProcessor,
            IRepository<SteamAccountEntity> accountsRepository,
            DebugSettings debugSettings,
            IWebDriver webDriver,
            ILogger logger)
        {
            this.gameCodeProcessor = gameCodeProcessor;
            this.steamProcessor = steamProcessor;
            this.accountsRepository = accountsRepository;
            this.debugSettings = debugSettings;
            this.webDriver = webDriver;
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
            LogInToSteam(account);
            RegisterOnGameCode(account);
            ClearHistory();
        }

        void LogInToSteam(SteamAccount account)
        {
            logger.Info(MyOperation.SteamLogIn, OperationStatus.Started, new LogInfo(MyLogInfoKey.Username, account.Username));

            try
            {
                steamProcessor.LogIn(account);
                logger.Debug(MyOperation.SteamLogIn, OperationStatus.Success, new LogInfo(MyLogInfoKey.Username, account.Username));
            }
            catch (Exception ex)
            {
                logger.Error(MyOperation.SteamLogIn, OperationStatus.Failure, ex, new LogInfo(MyLogInfoKey.Username, account.Username));
                throw;
            }
        }

        void RegisterOnGameCode(SteamAccount account)
        {
            logger.Info(MyOperation.GameCodeRegistration, OperationStatus.Started, new LogInfo(MyLogInfoKey.Username, account.Username));

            try
            {
                gameCodeProcessor.Register(account);
                gameCodeProcessor.LinkSteamAccount();

                logger.Debug(MyOperation.GameCodeRegistration, OperationStatus.Success, new LogInfo(MyLogInfoKey.Username, account.Username));
            }
            catch (Exception ex)
            {
                logger.Error(MyOperation.GameCodeRegistration, OperationStatus.Failure, ex.StackTrace, ex, new LogInfo(MyLogInfoKey.Username, account.Username));
                throw;
            }
        }

        void ClearHistory()
        {
            gameCodeProcessor.ClearCookies();
            steamProcessor.ClearCookies();

            webDriver.SwitchTo().Window(webDriver.WindowHandles[0]);
            foreach (string tab in webDriver.WindowHandles.Skip(1))
            {
                webDriver.SwitchTo().Window(webDriver.WindowHandles[0]);
                webDriver.Close();
            }
        }
    }
}
