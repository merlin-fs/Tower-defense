using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Model.World
{
    using Meshing;
    using Chunks;
    using Utilities;


    public interface IMapView
    {
        int2 Size { get; }
        IChunkView Chunk { get; }
        VoxelData.IDataStore DataStore { get; }
    }

    public class MapView : MonoBehaviour, IMapView, VoxelData.IDataStore
    {
        [SerializeField]
        Chunk m_Chunk = default;
        [SerializeField]
        bool _water = false;

        private Map.Data m_Map;
        private Chunk m_ChunkInst;

        #region IMapView
        int2 IMapView.Size => m_Map.Size;

        IChunkView IMapView.Chunk => m_Chunk;

        VoxelData.IDataStore IMapView.DataStore => this;
        #endregion

        
        bool VoxelData.IDataStore.TryGetVoxelDataChunk(out VoxelData.DataVolume chunk)
        {
            chunk = new VoxelData.DataVolume(
                TryGetVoxelData,
                m_Map.Size.x, m_Map.ViewData.HeightMax + 10, m_Map.Size.y);
            return true;
        }
        
        bool TryGetVoxelData(VoxelData.DataVolume data, int3 localPosition, out float voxelData)
        {
            voxelData = 0;
            int idx = m_Map.At(localPosition.x, localPosition.z);
            if (idx >= m_Map.Tiles.Heights.Count)
                return false;

            /*
            if (!_water)
            {
                if (m_HeightType[idx].Value == Map.Tile.HeightType.Type.DeepWater || m_HeightType[idx].Value == Map.Tile.HeightType.Type.ShallowWater)
                    return false;
            }
            else
            {
                if (!(m_HeightType[idx].Value == Map.Tile.HeightType.Type.DeepWater || m_HeightType[idx].Value == Map.Tile.HeightType.Type.ShallowWater))
                    return false;
            }
            */

            //float heightmapValue = (float)m_HeightType[idx].Value / 18f;
            float heightmapValue = m_Map.Tiles.Heights[idx].Value;
            float h = data.Depth * heightmapValue;
            voxelData = localPosition.y - h;
            return true;
        }

        public Transform InitMesh(Map.Data map)
        {
            m_Map = map;
            Debug.Log($"map size {m_Map.Size}");
            if (m_ChunkInst)
                Destroy(m_ChunkInst.gameObject);

            m_ChunkInst = Instantiate(m_Chunk, transform);
            IChunkView chunk = m_ChunkInst;

            chunk.Initialize(this);
            IMapMesher mesher = new Meshing.MarchingCubes.MarchingCubesMesher();
            chunk.GenerateMesh(mesher);
            return m_ChunkInst.gameObject.transform;
        }
    }
}