using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

namespace Game.Model.Units
{
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(GameLogicSystemGroup))]
    public partial class HealthSystem : SystemBase
    {
        private EntityQuery m_Query;
        private EntityCommandBufferSystem m_CommandBuffer;

        protected override void OnCreate()
        {
            m_CommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            m_Query = GetEntityQuery(
                ComponentType.ReadOnly<Health>(),
                ComponentType.ReadWrite<StateCalcProperty>()
            );
            RequireForUpdate(m_Query);
        }

        struct CalculateHealthJob : IJobEntityBatch
        {
            [ReadOnly]
            public EntityTypeHandle InputEntity;
            public EntityCommandBuffer.ParallelWriter Writer;
            public ComponentTypeHandle<Health> InputHealth;
            
            [ReadOnly]
            public float Delta;


            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var healts = batchInChunk.GetNativeArray(InputHealth);
                var entities = batchInChunk.GetNativeArray(InputEntity);

                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    Health health = healts[i];
                    if (health.Value <= 0)
                        Writer.AddComponent<StateDead>(batchIndex, entities[i]);
                    Writer.RemoveComponent<StateCalcProperty>(batchIndex, entities[i]);
                }
            }
        }

        protected override void OnUpdate()
        {
            var job = new CalculateHealthJob()
            {
                InputHealth = GetComponentTypeHandle<Health>(false),
                InputEntity = GetEntityTypeHandle(),
                Writer = m_CommandBuffer.CreateCommandBuffer().AsParallelWriter(),
                Delta = Time.DeltaTime,
            };
            Dependency = job.ScheduleParallel(m_Query, Dependency);
            m_CommandBuffer.AddJobHandleForProducer(Dependency);
        }
    }
}