using System;

namespace Game.Model.World.VoxelData
{
    public interface IDataStore
    {
        bool TryGetVoxelDataChunk(out DataVolume chunk);
    }
}
