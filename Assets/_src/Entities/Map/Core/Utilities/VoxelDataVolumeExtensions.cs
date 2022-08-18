using System;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace Game.Model.World.Utilities
{
    using Meshing.Data;
    using VoxelData;

    /// <summary>
    /// Extensions methods for <see cref="VoxelDataVolume"/>
    /// </summary>
    public static class VoxelDataVolumeExtensions
    {
        /// <summary>
        /// Gets a cube-shaped volume of voxel data from <paramref name="voxelDataVolume"/>. The size of the cube is 1 unit. 
        /// </summary>
        /// <param name="voxelDataVolume">The voxel data volume to get the voxel data from</param>
        /// <param name="localPosition">The origin of the cube</param>
        /// <returns>A cube-shaped volume of voxel data. The size of the cube is 1 unit.</returns>
        public static VoxelCorners<float> GetVoxelDataUnitCube(this DataVolume voxelDataVolume, int3 localPosition)
        {
            VoxelCorners<float> voxelDataCorners;

            if (voxelDataVolume.GetVoxelData(voxelDataVolume, localPosition, out float data))
                voxelDataCorners = new VoxelCorners<float>(data);
            else 
                return new VoxelCorners<float>(0);

            Parallel.For(0, 8,
                (i) =>
                {
                    int3 voxelCorner = localPosition + LookupTables.CubeCorners[i];
                    if (voxelDataVolume.GetVoxelData(voxelDataVolume, voxelCorner, out float voxelData))
                    {
                        voxelDataCorners[i] = voxelData;
                    }
                });
            return voxelDataCorners;
        }
    }
}