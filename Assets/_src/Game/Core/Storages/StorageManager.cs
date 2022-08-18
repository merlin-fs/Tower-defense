using System;
using Common.Core.Storages;
using Common.Core.Profiles;

namespace Game.Core.Storages
{
    public interface IStorageManager: IStorageManager<PlayerProfile>
    {

    }

    public class StorageManager : StorageManager<PlayerProfile>, IStorageManager
    {
    }
}
