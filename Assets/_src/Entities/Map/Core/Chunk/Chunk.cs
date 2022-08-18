using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Model.World.Chunks
{
    using Meshing;
    using Meshing.Data;

    public interface IChunkView
    {
        void Initialize(IMapView map);
        void GenerateMesh(IMapMesher mesher);
    }


    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
    public class Chunk : MonoBehaviour, IChunkView
    {
        private IMapView m_Map;

        private MeshFilter m_MeshFilter;

        private MeshCollider m_MeshCollider;

        /// <summary>
        /// Has the voxel data of this chunk been changed during the last frame
        /// </summary>
        public bool HasChanges { get; set; }

        private void Awake()
        {
            m_MeshFilter = GetComponent<MeshFilter>();
            m_MeshCollider = GetComponent<MeshCollider>();
        }

        void IChunkView.Initialize(IMapView map)
        {
            m_Map = map;
            name = "map_mesh";
        }
       
        void IChunkView.GenerateMesh(IMapMesher mesher)
        {
            if (!m_Map.DataStore.TryGetVoxelDataChunk(out VoxelData.DataVolume data))
                return;
            JobHandleWithData<IMesherJob> jobHandle = mesher.CreateMesh(data);
            if (jobHandle == null) 
                return;

            IMesherJob job = jobHandle.JobData;

            Mesh mesh = new Mesh();
            SubMeshDescriptor subMesh = new SubMeshDescriptor(0, 0);

            jobHandle.JobHandle.Complete();

            int vertexCount = job.VertexCountCounter.Count * 3;
            job.VertexCountCounter.Dispose();

            mesh.SetVertexBufferParams(vertexCount, MeshingVertexData.VertexBufferMemoryLayout);
            mesh.SetIndexBufferParams(vertexCount, IndexFormat.UInt32);

            mesh.SetVertexBufferData(job.OutputVertices, 0, 0, vertexCount, 0, MeshUpdateFlags.DontValidateIndices);
            mesh.SetIndexBufferData(job.OutputTriangles, 0, 0, vertexCount, MeshUpdateFlags.DontValidateIndices);

            job.OutputVertices.Dispose();
            job.OutputTriangles.Dispose();

            mesh.subMeshCount = 1;
            subMesh.indexCount = vertexCount;
            mesh.SetSubMesh(0, subMesh);

            mesh.RecalculateBounds();

            m_MeshFilter.sharedMesh = mesh;
            m_MeshCollider.sharedMesh = mesh;
            HasChanges = false;
        }

        /// <summary>
        /// Generates a chunk name from a chunk coordinate
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk</param>
        /// <returns>The name of the chunk</returns>
        public static string GetName(int3 chunkCoordinate)
        {
            return $"Chunk_{chunkCoordinate.x.ToString()}_{chunkCoordinate.y.ToString()}_{chunkCoordinate.z.ToString()}";
        }
    }
}