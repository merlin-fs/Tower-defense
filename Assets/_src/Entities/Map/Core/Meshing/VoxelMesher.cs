using Unity.Mathematics;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace Game.Model.World.Meshing
{
    public class JobHandleWithData<T>
    {
        /// <summary>
        /// The job handle
        /// </summary>
        public JobHandle JobHandle { get; set; }

        /// <summary>
        /// The associated data
        /// </summary>
        public T JobData { get; set; }
    }

    public interface IMapMesher
    {
        JobHandleWithData<IMesherJob> CreateMesh(VoxelData.DataVolume voxelDataStore);
    }

    public abstract class VoxelMesher : MonoBehaviour
    {
        public abstract JobHandleWithData<IMesherJob> CreateMesh(VoxelData.DataVolume voxelDataStore);
    }
}