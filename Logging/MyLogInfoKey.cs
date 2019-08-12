using NuciLog.Core;

namespace GameCodeAccountCreator.Logging
{
    public sealed class MyLogInfoKey : LogInfoKey
    {
        MyLogInfoKey(string name)
            : base(name)
        {
            
        }

        public static LogInfoKey Username => new MyLogInfoKey(nameof(Username));
    }
}
