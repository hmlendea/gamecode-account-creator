using GameCodeAccountCreator.Service.Models;

namespace GameCodeAccountCreator.Service.Processors
{
    public interface ISteamProcessor
    {
        void LogIn(SteamAccount account);
    }
}
