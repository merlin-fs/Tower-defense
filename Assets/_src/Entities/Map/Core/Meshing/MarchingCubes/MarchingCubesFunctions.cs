using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Mathematics;

namespace Game.Model.World.Meshing.MarchingCubes
{
    /// <summary>
    /// A collection of Marching Cubes -related functions
    /// </summary>
    public static class MarchingCubesFunctions
    {
        /// <summary>
        /// Gets the corners for the voxel at a position
        /// </summary>
        /// <param name="position">The position of the voxel</param>
        /// <returns>The corners of the voxel</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Data.VoxelCorners<int3> GetCorners(int3 position)
        {
            Data.VoxelCorners<int3> corners = new Data.VoxelCorners<int3>();
            Parallel.For(0, 8,
                (i) =>
                {
                    corners[i] = position + Utilities.LookupTables.CubeCorners[i];
                });
            return corners;
        }

        /// <summary>
        /// Interpolates the vertex's position 
        /// </summary>
        /// <param name="p1">The first corner's position</param>
        /// <param name="p2">The second corner's position</param>
        /// <param name="v1">The first corner's density</param>
        /// <param name="v2">The second corner's density</param>
        /// <param name="isolevel">The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)</param>
        /// <returns>The interpolated vertex's position</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 VertexInterpolate(float3 p1, float3 p2, float v1, float v2, float isolevel)
        {
            return p1 + (isolevel - v1) * (p2 - p1) / (v2 - v1);
        }

        /// <summary>
        /// Calculates the cube index of a single voxel
        /// </summary>
        /// <param name="voxelDensities">The densities of the voxel</param>
        /// <param name="isolevel">The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)</param>
        /// <returns>The calculated cube index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte CalculateCubeIndex(Data.VoxelCorners<float> voxelDensities, float isolevel)
        {
            byte cubeIndex = 0;
          
            if (voxelDensities.Corner1 < isolevel) { cubeIndex |= 1; }
            if (voxelDensities.Corner2 < isolevel) { cubeIndex |= 2; }
            if (voxelDensities.Corner3 < isolevel) { cubeIndex |= 4; }
            if (voxelDensities.Corner4 < isolevel) { cubeIndex |= 8; }
            if (voxelDensities.Corner5 < isolevel) { cubeIndex |= 16; }
            if (voxelDensities.Corner6 < isolevel) { cubeIndex |= 32; }
            if (voxelDensities.Corner7 < isolevel) { cubeIndex |= 64; }
            if (voxelDensities.Corner8 < isolevel) { cubeIndex |= 128; }
            return cubeIndex;
        }

        /// <summary>
        /// Generates the vertex list for a single voxel
        /// </summary>
        /// <param name="voxelDensities">The densities of the voxel</param>
        /// <param name="voxelCorners">The corners of the voxel</param>
        /// <param name="edgeIndex">The edge index</param>
        /// <param name="isolevel">The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)</param>
        /// <returns>The generated vertex list for the voxel</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VertexList GenerateVertexList(Data.VoxelCorners<float> voxelDensities, Data.VoxelCorners<int3> voxelCorners, int edgeIndex, float isolevel)
        {
            VertexList vertexList = new VertexList();

            Parallel.For(0, 12,
                (i) =>
                {
                    if ((edgeIndex & (1 << i)) == 0)
                        return; //continue;

                    int edgeStartIndex = Utilities.LookupTables.EdgeIndexTable[2 * i + 0];
                    int edgeEndIndex = Utilities.LookupTables.EdgeIndexTable[2 * i + 1];

                    int3 corner1 = voxelCorners[edgeStartIndex];
                    int3 corner2 = voxelCorners[edgeEndIndex];

                    float density1 = voxelDensities[edgeStartIndex];
                    float density2 = voxelDensities[edgeEndIndex];

                    vertexList[i] = VertexInterpolate(corner1, corner2, density1, density2, isolevel);
                });
            return vertexList;
        }
    }
}