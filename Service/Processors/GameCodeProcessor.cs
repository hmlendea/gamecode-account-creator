using System;

using NuciWeb;

using OpenQA.Selenium;

using GameCodeAccountCreator.Service.Models;

namespace GameCodeAccountCreator.Service.Processors
{
    public sealed class GameCodeProcessor : IGameCodeProcessor
    {
        public string HomePageUrl => "https://gamecode.win/";
        public string LogInUrl => $"{HomePageUrl}/login";
        public string AccountsUrl => $"{HomePageUrl}/users/accounts/all";

        readonly IWebProcessor webProcessor;

        public GameCodeProcessor(IWebProcessor webProcessor)
        {
            this.webProcessor = webProcessor;
        }

        public void Register(SteamAccount account)
        {
            webProcessor.GoToUrl(LogInUrl);

            By popupSelector = By.XPath(@"//cloudflare-app[1]");
            By popupCloseButtonSelector = By.XPath(@"/html/body/cloudflare-app[1]/div/div[3]/a");
            
            By registrationTabSelector = By.Id("_register");
            By usernameInputSelector = By.XPath(@"//*[@id='registerForm']/form/input[@name='name']");
            By emailInputSelector = By.XPath(@"//*[@id='registerForm']/form/input[@name='email']");
            By password1InputSelector = By.XPath(@"//*[@id='registerForm']/form/input[@name='password']");
            By password2InputSelector = By.XPath(@"//*[@id='registerForm']/form/input[@name='password_confirmation']");
            By artsCardSelector = By.Id("text_arts");

            webProcessor.Click(registrationTabSelector);

            webProcessor.WaitForElementToExist(popupCloseButtonSelector, TimeSpan.FromSeconds(1));
            if (webProcessor.IsElementVisible(popupSelector))
            {
                webProcessor.Click(popupSelector);
                webProcessor.Click(popupCloseButtonSelector);
            }

            webProcessor.SetText(usernameInputSelector, account.Username);
            webProcessor.SetText(emailInputSelector, $"{account.Username}@yopmail.com");
            webProcessor.SetText(password1InputSelector, account.Password);
            webProcessor.SetText(password2InputSelector, account.Password);

            webProcessor.WaitForElementToExist(artsCardSelector, waitIndefinetely: true);
        }

        public void LinkSteamAccount()
        {
            webProcessor.GoToUrl(AccountsUrl);

            By steamConnectionButtonSelector = By.XPath(@"//i[contains(@class,'fa-steam')]/../a/button");
            By logInButtonSelector = By.Id("imageLogin");
            By gameCard1Selector = By.Id("gamesToggle_1");
            By kinguinAlertSelector = By.ClassName("edrone--push--alert-container");

            webProcessor.Click(steamConnectionButtonSelector);

            // TODO: Workaround the annoying redirect ad
            webProcessor.Wait();
            webProcessor.NewTab(AccountsUrl);
            webProcessor.Click(steamConnectionButtonSelector);

            webProcessor.Click(logInButtonSelector);

            webProcessor.WaitForElementToExist(gameCard1Selector);
        }
    }
}
