using NuciWeb;

using OpenQA.Selenium;

using GameCodeAccountCreator.Service.Models;

namespace GameCodeAccountCreator.Service.Processors
{
    public sealed class SteamProcessor : WebProcessor, ISteamProcessor
    {
        public string HomePageUrl => "https://store.steampowered.com";
        public string LoginUrl => $"{HomePageUrl}/login/?redir=&redir_ssl=1";

        readonly SteamAccount account;

        public SteamProcessor(
            IWebDriver driver,
            SteamAccount account)
            : base(driver)
        {
            this.account = account;
        }
        
        public void LogIn()
        {
            GoToUrl(LoginUrl);

            By usernameSelector = By.Id("input_username");
            By passwordSelector = By.Id("input_password");
            By avatarSelector = By.XPath(@"//a[contains(@class,'playerAvatar')]");
            By loginButtonSelector = By.XPath(@"//*[@id='login_btn_signin']/button");

            SetText(usernameSelector, account.Username);
            SetText(passwordSelector, account.Password);
            
            Click(loginButtonSelector);
            
            WaitForElementToExist(avatarSelector);
        }
    }
}
