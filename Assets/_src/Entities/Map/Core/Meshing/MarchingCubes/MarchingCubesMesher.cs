using System;
using Unity.Collections;
using System.Threading.Tasks;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Model.World.Meshing.MarchingCubes
{
    /// <summary>
    /// A mesher for the marching cubes algorithm
    /// </summary>
    public class MarchingCubesMesher : IMapMesher
    {
        /// <summary>
        /// The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)
        /// </summary>
        ///[SerializeField, Range(0, 1)] 
        private float m_Isolevel = 0.8f;

        /// <summary>
        /// The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)
        /// </summary>
        public float Isolevel => m_Isolevel;

        /// <summary>
        /// Starts a mesh generation job
        /// </summary>
        /// <param name="voxelDataStore">The store where to retrieve the voxel data from</param>
        /// <param name="chunkCoordinate">The coordinate of the chunk that will be generated</param>
        /// <returns>The job handle and the actual mesh generation job</returns>
        JobHandleWithData<IMesherJob> IMapMesher.CreateMesh(VoxelData.DataVolume voxelData)//
        {
            NativeCounter vertexCountCounter = new NativeCounter(Allocator.TempJob);
            int voxelCount = (voxelData.Width - 1) * (voxelData.Depth - 1) * (voxelData.Height - 1);
            int maxLength = 15 * voxelCount;

            NativeArray<Data.MeshingVertexData > outputVertices = new NativeArray<Data.MeshingVertexData>(maxLength, Allocator.TempJob);
            NativeArray<uint> outputTriangles = new NativeArray<uint>(maxLength, Allocator.TempJob);

            MarchingCubesJob marchingCubesJob = new MarchingCubesJob
            {
                VoxelData = voxelData,
                Isolevel = Isolevel,
                VertexCountCounter = vertexCountCounter,

                OutputVertices = outputVertices,
                OutputTriangles = outputTriangles
            };

            /*
            Parallel.For(0, voxelCount,
                (i) =>
                {
                    marchingCubesJob.Execute(i);
                });
            */
            JobHandle jobHandle = marchingCubesJob.Schedule(voxelCount, 128);
            JobHandleWithData<IMesherJob> jobHandleWithData = new JobHandleWithData<IMesherJob>()
            {
                JobHandle = jobHandle,
                JobData = marchingCubesJob,
            };
            return jobHandleWithData;
        }
    }
}
