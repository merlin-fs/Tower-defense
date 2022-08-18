using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace Game.Model.World
{
    public partial struct Map : IComponentData, IDisposable
    {
        private GCHandle m_ProperiesHandle;

        public int2 Size;
        public ViewDataStruct ViewData;
        public TilesData Tiles => (TilesData)m_ProperiesHandle.Target;

        public struct ViewDataStruct
        {
            public float4x4 WorldToLocalMatrix;
            public float4x4 LocalToWorldMatrix;
            public Bounds Bounds;
            public int HeightMax;
        }

        public void InitView(float4x4 worldToLocalMatrix, float4x4 localToWorldMatrix, Bounds bounds)
        {
            ViewData.WorldToLocalMatrix = worldToLocalMatrix;
            ViewData.LocalToWorldMatrix = localToWorldMatrix;
            ViewData.Bounds = bounds;
        }

        public Map(int2 size, int heightMax, Initialization initialization)
        {
            Size = size;
            ViewData = new ViewDataStruct() { HeightMax = heightMax, };
            m_ProperiesHandle = TilesData.Create();
            Tiles.Init(size.x * size.y);

            initialization?.Invoke(Tiles, size);
        }

        public void Dispose()
        {
            Tiles.Dispose();
            m_ProperiesHandle.Free();
        }
    }
}
