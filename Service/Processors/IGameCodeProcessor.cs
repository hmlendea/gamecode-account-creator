using GameCodeAccountCreator.Service.Models;

namespace GameCodeAccountCreator.Service.Processors
{
    public interface IGameCodeProcessor
    {
        void Register(SteamAccount account);

        void LinkSteamAccount();
    }
}
