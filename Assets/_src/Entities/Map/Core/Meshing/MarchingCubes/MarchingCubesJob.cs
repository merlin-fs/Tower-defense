using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Game.Model.World.Meshing.MarchingCubes
{
    using Utilities;

    /// <summary>
    /// A marching cubes mesh generation job
    /// </summary>
    //[BurstCompile]
    public struct MarchingCubesJob : IMesherJob
    {
        /// <summary>
        /// The densities to generate the mesh off of
        /// </summary>
        [ReadOnly] 
        private VoxelData.DataVolume _voxelData;

        /// <summary>
        /// The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)
        /// </summary>
        public float Isolevel { get; set; }

        /// <summary>
        /// The counter to keep track of the triangle index
        /// </summary>
        public NativeCounter VertexCountCounter { get; set; }

        /// <summary>
        /// The generated vertices
        /// </summary>
        [NativeDisableParallelForRestriction, WriteOnly] 
        private NativeArray<Data.MeshingVertexData> _vertices;

        /// <summary>
        /// The generated triangles
        /// </summary>
        [NativeDisableParallelForRestriction, WriteOnly]
        //private NativeArray<ushort> _triangles;
        private NativeArray<uint> _triangles;

        /// <summary>
        /// The voxel data to generate the mesh from
        /// </summary>
        public VoxelData.DataVolume VoxelData { get => _voxelData; set => _voxelData = value; }

        /// <summary>
        /// The generated vertices
        /// </summary>
        public NativeArray<Data.MeshingVertexData> OutputVertices { get => _vertices; set => _vertices = value; }

        /// <summary>
        /// The generated triangles
        /// </summary>
        public NativeArray<uint> OutputTriangles { get => _triangles; set => _triangles = value; }

        /// <summary>
        /// The execute method required by the Unity Job System's IJobParallelFor
        /// </summary>
        /// <param name="index">The iteration index</param>
        public void Execute(int index)
        {
            // The position of the voxel Voxel inside the chunk. Goes from (0, 0, 0) to (densityVolume.Width-1, densityVolume.Height-1, densityVolume.Depth-1). Both are inclusive.
            int3 voxelLocalPosition = Utilities.IndexUtilities.IndexToXyz(index, _voxelData.Width - 1, _voxelData.Depth - 1);
            Data.VoxelCorners<float> densities = _voxelData.GetVoxelDataUnitCube(voxelLocalPosition);

            byte cubeIndex = MarchingCubesFunctions.CalculateCubeIndex(densities, Isolevel);
            if (cubeIndex == 0 || cubeIndex == 255)
            {
                return;
            }

            Data.VoxelCorners<int3> corners = MarchingCubesFunctions.GetCorners(voxelLocalPosition);

            int edgeIndex = LookupTables.EdgeTable[cubeIndex];

            VertexList vertexList = MarchingCubesFunctions.GenerateVertexList(densities, corners, edgeIndex, Isolevel);

            // Index at the beginning of the row
            int rowIndex = 15 * cubeIndex;

            for (int i = 0; LookupTables.TriangleTable[rowIndex+i] != -1 && i < 15; i += 3)
            {
                float3 vertex1 = vertexList[LookupTables.TriangleTable[rowIndex + i + 0]];
                float3 vertex2 = vertexList[LookupTables.TriangleTable[rowIndex + i + 1]];
                float3 vertex3 = vertexList[LookupTables.TriangleTable[rowIndex + i + 2]];

                if (!vertex1.Equals(vertex2) && !vertex1.Equals(vertex3) && !vertex2.Equals(vertex3))
                {
                    float3 normal = math.normalize(math.cross(vertex2 - vertex1, vertex3 - vertex1));

                    int triangleIndex = VertexCountCounter.Increment() * 3;
                    
                    _vertices[triangleIndex + 0] = new Data.MeshingVertexData(vertex1, normal);
                    _triangles[triangleIndex + 0] = (uint)(triangleIndex + 0);

                    _vertices[triangleIndex + 1] = new Data.MeshingVertexData(vertex2, normal);
                    _triangles[triangleIndex + 1] = (uint)(triangleIndex + 1);

                    _vertices[triangleIndex + 2] = new Data.MeshingVertexData(vertex3, normal);
                    _triangles[triangleIndex + 2] = (uint)(triangleIndex + 2);
                }
            }
        }

    }
}