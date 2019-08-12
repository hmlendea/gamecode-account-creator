using System.Collections.Generic;
using System.Linq;

using GameCodeAccountCreator.DataAccess.DataObjects;
using GameCodeAccountCreator.Service.Models;

namespace GameCodeAccountCreator.Service.Mapping
{
    internal static class SteamAccountMapping
    {
        internal static SteamAccount ToServiceModel(this SteamAccountEntity dataObject)
        {
            SteamAccount serviceModel = new SteamAccount();
            serviceModel.Username = dataObject.Username;
            serviceModel.Password = dataObject.Password;

            return serviceModel;
        }

        internal static IEnumerable<SteamAccount> ToServiceModels(this IEnumerable<SteamAccountEntity> dataObjects)
        {
            IEnumerable<SteamAccount> serviceModels = dataObjects.Select(dataObject => dataObject.ToServiceModel());

            return serviceModels;
        }
    }
}
