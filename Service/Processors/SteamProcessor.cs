using System.Security.Authentication;

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

        public SteamProcessor(IWebProcessor webProcessor)
        {
            this.webProcessor = webProcessor;
        }
        
        public void LogIn(SteamAccount account)
        {
            webProcessor.GoToUrl(LoginUrl);

            By usernameSelector = By.Id("input_username");
            By passwordSelector = By.Id("input_password");
            By captchaInputSelector = By.Id("input_captcha");
            By logInButtonSelector = By.XPath(@"//*[@id='login_btn_signin']/button");
            By steamGuardCodeInputSelector = By.Id("twofactorcode_entry");
            By avatarSelector = By.XPath(@"//a[contains(@class,'playerAvatar')]");
            
            if (webProcessor.IsElementVisible(captchaInputSelector))
            {
                throw new AuthenticationException("Captcha input required");
            }

            webProcessor.SetText(usernameSelector, account.Username);
            webProcessor.SetText(passwordSelector, account.Password);

            webProcessor.Click(logInButtonSelector);
            webProcessor.WaitForAnyElementToBeVisible(steamGuardCodeInputSelector, avatarSelector);

            if (webProcessor.IsElementVisible(steamGuardCodeInputSelector))
            {
                throw new AuthenticationException("SteamGuard input required");
            }
        }
    }
}
