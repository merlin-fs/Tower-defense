using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using Debug = UnityEngine.Debug;

namespace Game.Model.World
{

    public partial struct Map
    {
        public struct GenerateMapTag : ISystemStateComponentData
        {
            [NativeDisableUnsafePtrRestriction]
            private readonly IntPtr m_Initialization;
            public Initialization InitProperties => System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<Initialization>(m_Initialization);

            public int TerrainOctaves;
            public float TerrainFrequency;
            public int Seed;
            public int2 Size;
            public int HeightMax;

            public GenerateMapTag(int octaves, float frequency, int2 size, int seed, int heightMax, Initialization initialization)
            {
                TerrainOctaves = octaves;
                TerrainFrequency = frequency;
                Size = size;
                Seed = seed;
                HeightMax = heightMax;
                m_Initialization = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(initialization);
            }
        };

        public struct DisposeMapTag : ISystemStateComponentData
        {
        };

        public partial class Generate : SystemBase
        {
            private EntityCommandBufferSystem m_CommandBuffer;
            private EntityQuery m_Query;

            protected override void OnCreate()
            {
                m_CommandBuffer = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();

                m_Query = GetEntityQuery(
                    ComponentType.ReadOnly<GenerateMapTag>()
                );
                RequireForUpdate(m_Query);
            }

            protected override void OnUpdate()
            {
                //var tt = new System.Diagnostics.Stopwatch();
                //tt.Start();

                var configs = m_Query.ToComponentDataArray<GenerateMapTag>(Allocator.TempJob);
                var buffer = m_CommandBuffer.CreateCommandBuffer();

                NativeArray<Map> maps = new NativeArray<Map>(configs.Length, Allocator.TempJob);
                
                for (var i = 0; i < maps.Length; i++)
                    maps[i] = new Map(configs[i].Size, configs[i].HeightMax, configs[i].InitProperties);

                Dependency = new InitJob()
                {
                    Maps = maps,
                    Configs = configs,
                    Writer = buffer.AsParallelWriter(),

                }.Schedule(configs.Length, 10, Dependency);

                //m_CommandBuffer.AddJobHandleForProducer(Dependency);
                Dependency.Complete();
                buffer.RemoveComponentForEntityQuery<GenerateMapTag>(m_Query);
                buffer.DestroyEntitiesForEntityQuery(m_Query);

                //tt.Stop();
                //Debug.Log("Init: " + tt.Elapsed);
            }
        }
    }
}
