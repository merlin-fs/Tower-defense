using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;


namespace Game.Model
{
    using World;
    using Core;


    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    public partial class TestSpawnSystem : SystemBase
    {
        private EntityCommandBufferSystem m_CommandBuffer;

        private EntityQuery m_Query;
        protected override void OnCreate()
        {
            m_CommandBuffer = World.GetOrCreateSystemManaged<GameSpawnSystemCommandBufferSystem>();

            m_Query = GetEntityQuery(
                ComponentType.ReadOnly<TestSpawn.SpawnState>(),
                ComponentType.ReadOnly<Teams>()
            );
            RequireForUpdate(m_Query);
        }

        protected override void OnUpdate()
        {
            var buff = m_CommandBuffer.CreateCommandBuffer();
            var configs = m_Query.ToEntityArray(Unity.Collections.Allocator.Temp);

            try
            {
                foreach(var iter in configs)
                {
                    var team = EntityManager.GetComponentData<Teams>(iter);
                    var config = EntityManager.GetComponentData<TestSpawn.SpawnState>(iter);
                    //var position = EntityManager.GetComponentData<SetPositionOnMap>(iter);
                    
                    Entity entity = buff.Instantiate(config.Prefab);
                    //float3 pos = map.MapToWord(position.InitPosition);

                    //buff.SetComponent<Translation>(entity, new Translation() { Value = pos });
                    //buff.SetComponent<SetPositionOnMap>(entity, position);
                    buff.AddComponent<Teams>(entity, team);
                    buff.AddComponent<StateInit>(entity);
                    //buff.AddComponent<Disabled>(entity);
                }
            }
            finally
            {
                configs.Dispose();

                buff.RemoveComponentForEntityQuery<TestSpawn.SpawnState>(m_Query);
                buff.DestroyEntitiesForEntityQuery(m_Query);
            }
        }
    }
}