using NuciWeb;

namespace GameCodeAccountCreator.Service.Processors
{
    public interface ISteamProcessor : IWebProcessor
    {
        void LogIn();
    }
}
