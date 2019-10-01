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
        readonly IGameCodeProcessor gameCodeProcessor;
        readonly ISteamProcessor steamProcessor;
        readonly IRepository<SteamAccountEntity> accountsRepository;
        readonly DebugSettings debugSettings;
        readonly ILogger logger;

        public AccountCreator(
            IGameCodeProcessor gameCodeProcessor,
            ISteamProcessor steamProcessor,
            IRepository<SteamAccountEntity> accountsRepository,
            DebugSettings debugSettings,
            ILogger logger)
        {
            this.gameCodeProcessor = gameCodeProcessor;
            this.steamProcessor = steamProcessor;
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
            LogInToSteam(account);
            RegisterOnGameCode(account);
        }

        void LogInToSteam(SteamAccount account)
        {
            logger.Info(MyOperation.SteamLogIn, OperationStatus.Started, new LogInfo(MyLogInfoKey.Username, account.Username));

            try
            {
                steamProcessor.LogIn();
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
                gameCodeProcessor.Register();
                gameCodeProcessor.LinkSteamAccount();

                logger.Debug(MyOperation.GameCodeRegistration, OperationStatus.Success, new LogInfo(MyLogInfoKey.Username, account.Username));
            }
            catch (Exception ex)
            {
                logger.Error(MyOperation.GameCodeRegistration, OperationStatus.Failure, ex.StackTrace, ex, new LogInfo(MyLogInfoKey.Username, account.Username));
                throw;
            }
        }
    }
}
