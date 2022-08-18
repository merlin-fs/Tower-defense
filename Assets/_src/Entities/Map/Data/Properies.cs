using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.InteropServices;

namespace Game.Model.World
{
    public partial struct Map
    {
        public delegate void Initialization(TilesData value, int2 size);

        public partial class TilesData : IDisposable
        {
            public IReadOnlyList<Height> Heights => m_Heights;
            public IReadOnlyList<HeightType> HeightTypes => m_HeightTypes;
            public IReadOnlyList<Bitmask> Bitmasks => m_Bitmasks;

            public IList<Height> WriteHeights => m_Heights;
            public IList<HeightType> WriteHeightTypes => m_HeightTypes;
            public IList<Bitmask> WriteBitmasks => m_Bitmasks;


            private Height[] m_Heights;
            private HeightType[] m_HeightTypes;
            private Bitmask[] m_Bitmasks;

            public static GCHandle Create()
            {
                return GCHandle.Alloc(new TilesData());
            }

            public void Init(int length)
            {
                m_Heights = new Height[length];
                m_HeightTypes = new HeightType[length];
                m_Bitmasks = new Bitmask[length];
            }

            public virtual void Dispose()
            {
                m_Heights = null;
                m_HeightTypes = null;
                m_Bitmasks = null;
            }
        }
    }
}
