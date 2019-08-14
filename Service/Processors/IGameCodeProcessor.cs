using NuciWeb;

namespace GameCodeAccountCreator.Service.Processors
{
    public interface IGameCodeProcessor : IWebProcessor
    {
        void Register();

        void LinkSteamAccount();
    }
}
