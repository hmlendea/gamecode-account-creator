using NuciWeb;

using OpenQA.Selenium;
using OpenQA.Selenium.Support;

using GameCodeAccountCreator.Service.Models;

namespace GameCodeAccountCreator.Service.Processors
{
    public sealed class GameCodeProcessor : WebProcessor, IGameCodeProcessor
    {
        public string HomePageUrl => "https://gamecode.win/";
        public string LogInUrl => $"{HomePageUrl}/login";
        public string AccountsUrl => $"{HomePageUrl}/users/accounts/all";

        readonly SteamAccount account;

        public GameCodeProcessor(
            IWebDriver driver,
            SteamAccount account)
            : base(driver)
        {
            this.account = account;
        }

        public void Register()
        {
            GoToUrl(LogInUrl);

            By registrationTabSelector = By.Id("_register");
            By closeNotificationButtoNSelector = By.XPath(@"//*[@class='csa-close-button']/a");
            By usernameInputSelector = By.XPath(@"//*[@id='registerForm']/form/input[@name='name']");
            By emailInputSelector = By.XPath(@"//*[@id='registerForm']/form/input[@name='email']");
            By password1InputSelector = By.XPath(@"//*[@id='registerForm']/form/input[@name='password']");
            By password2InputSelector = By.XPath(@"//*[@id='registerForm']/form/input[@name='password_confirmation']");
            By artsCardSelector = By.Id("text_arts");

            Click(registrationTabSelector);
            Click(closeNotificationButtoNSelector);

            SetText(usernameInputSelector, account.Username);
            SetText(emailInputSelector, $"{account.Username}@yopmail.com");
            SetText(password1InputSelector, account.Password);
            SetText(password2InputSelector, account.Password);

            WaitForElementToExist(artsCardSelector, waitIndefinetely: true);
        }

        public void LinkSteamAccount()
        {
            GoToUrl(AccountsUrl);

            By steamConnectionButtonSelector = By.XPath(@"//i[contains(@class,'fa-steam')]/../a/button");
            By logInButtonSelector = By.Id("imageLogin");
            By gameCard1Selector = By.Id("gamesToggle_1");
            By kinguinAlertSelector = By.ClassName("edrone--push--alert-container");

            Click(steamConnectionButtonSelector);

            // TODO: Workaround the annoying redirect ad
            Wait();
            NewTab(AccountsUrl);
            Click(steamConnectionButtonSelector);

            Click(logInButtonSelector);

            WaitForElementToExist(gameCard1Selector);
        }
    }
}
