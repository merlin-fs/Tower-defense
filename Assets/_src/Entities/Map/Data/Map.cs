using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Common.Core;

namespace Game.Model.World
{
    public partial class Map
    {
        public struct ViewDataStruct
        {
            public float4x4 WorldToLocalMatrix;
            public float4x4 LocalToWorldMatrix;
            public Bounds Bounds;
            public int HeightMax;
        }

        public partial struct Data: IComponentData, IDisposable
        {
            public int2 Size;
            public ViewDataStruct ViewData;
            public ReferenceObject<TilesData> m_TilesData;
            public TilesData Tiles => m_TilesData.Link;

            public bool IsInit()
            {
                return math.all(Size != 0);
            }

            public void InitView(float4x4 worldToLocalMatrix, float4x4 localToWorldMatrix, Bounds bounds)
            {
                ViewData.WorldToLocalMatrix = worldToLocalMatrix;
                ViewData.LocalToWorldMatrix = localToWorldMatrix;
                ViewData.Bounds = bounds;
            }
            public Data(int2 size, int heightMax, Initialization initialization)
            {
                Size = size;
                ViewData = new ViewDataStruct() { HeightMax = heightMax, };
                m_TilesData = new ReferenceObject<TilesData>(new TilesData());
                Tiles.Init(size.x * size.y);

                initialization?.Invoke(Tiles, size);
            }

            public void Dispose()
            {
                m_TilesData.Dispose();
            }
        }
    }
}
