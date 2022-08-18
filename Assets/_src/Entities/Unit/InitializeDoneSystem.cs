using System;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

namespace Game.Model.Units
{
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(GameSpawnSystemGroup), OrderLast = true)]
    public partial class InitializeDoneSystem : SystemBase
    {
        private EntityQuery m_Query;
        private EntityCommandBufferSystem m_CommandBuffer;

        protected override void OnCreate()
        {
            m_CommandBuffer = World.GetOrCreateSystem<GameSpawnSystemCommandBufferSystem>();
            m_Query = GetEntityQuery(
                ComponentType.ReadOnly<StateInit>()
            );
            RequireForUpdate(m_Query);
        }

        protected override void OnUpdate()
        {
            m_CommandBuffer.CreateCommandBuffer().RemoveComponentForEntityQuery<StateInit>(m_Query);
        }
    }
}