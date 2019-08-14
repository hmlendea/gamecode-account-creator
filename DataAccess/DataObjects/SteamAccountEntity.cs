using NuciDAL.DataObjects;

namespace GameCodeAccountCreator.DataAccess.DataObjects
{
    public sealed class SteamAccountEntity : EntityBase
    {
        public string Username { get; set; }
        
        public string Password { get; set; }
    }
}
