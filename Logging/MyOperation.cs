using NuciLog.Core;

namespace GameCodeAccountCreator.Logging
{
    public sealed class MyOperation : Operation
    {
        MyOperation(string name)
            : base(name)
        {
            
        }

        public static Operation SteamLogIn => new MyOperation(nameof(SteamLogIn));

        public static Operation GameCodeRegistration => new MyOperation(nameof(GameCodeRegistration));
    }
}
