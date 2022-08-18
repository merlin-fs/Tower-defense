using Unity.Collections;
using Unity.Jobs;

namespace Game.Model.World.Meshing
{
    using Data;
    /// <summary>
    /// An interface for all the jobs that can extract a surface from voxel data
    /// </summary>
    public interface IMesherJob : IJobParallelFor
    {
        /// <summary>
        /// A counter that keeps track of how many vertices there are
        /// </summary>
        NativeCounter VertexCountCounter { get; set; }

        /// <summary>
        /// The voxel data to generate the mesh from
        /// </summary>
        VoxelData.DataVolume VoxelData { get; set; }

        /// <summary>
        /// The generated vertices
        /// </summary>
        NativeArray<MeshingVertexData> OutputVertices { get; set; }

        /// <summary>
        /// The generated triangles
        /// </summary>
        NativeArray<uint> OutputTriangles { get; set; }
    }
}