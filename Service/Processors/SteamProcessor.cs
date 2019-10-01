using NuciWeb;

using OpenQA.Selenium;

using GameCodeAccountCreator.Service.Models;

namespace GameCodeAccountCreator.Service.Processors
{
    public sealed class SteamProcessor : ISteamProcessor
    {
        public string HomePageUrl => "https://store.steampowered.com";
        public string LoginUrl => $"{HomePageUrl}/login/?redir=&redir_ssl=1";

        readonly IWebProcessor webProcessor;
        readonly SteamAccount account;

        public SteamProcessor(
            IWebProcessor webProcessor,
            SteamAccount account)
        {
            this.account = account;
        }
        
        public void LogIn()
        {
            webProcessor.GoToUrl(LoginUrl);

            By usernameSelector = By.Id("input_username");
            By passwordSelector = By.Id("input_password");
            By avatarSelector = By.XPath(@"//a[contains(@class,'playerAvatar')]");
            By loginButtonSelector = By.XPath(@"//*[@id='login_btn_signin']/button");

            webProcessor.SetText(usernameSelector, account.Username);
            webProcessor.SetText(passwordSelector, account.Password);

            webProcessor.Click(loginButtonSelector);
            webProcessor.WaitForElementToExist(avatarSelector);
        }
    }
}
