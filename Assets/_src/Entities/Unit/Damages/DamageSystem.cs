using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

namespace Game.Model.Damages
{
    using Model.Properties;


    [UpdateInGroup(typeof(GameLogicSystemGroup), OrderFirst = true)]
    public partial class DamageSimpleSystem : SystemBase
    {
        private EntityQuery m_Query;
        private EntityCommandBufferSystem m_CommandBuffer;

        protected override void OnCreate()
        {
            m_CommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            m_Query = GetEntityQuery(
                ComponentType.ReadWrite<StateShotDone>(),
                ComponentType.ReadWrite<DamageSimple>(),
                ComponentType.ReadWrite<Health>()
            );

            RequireForUpdate(m_Query);
        }


        struct ProduceDamageJob : IJobEntityBatch
        {
            [ReadOnly]
            public EntityTypeHandle InputEntity;

            public ComponentTypeHandle<StateShotDone> InputShot;
            public ComponentTypeHandle<DamageSimple> InputDamage;

            public EntityCommandBuffer.ParallelWriter Writer;
            public ComponentTypeHandle<Health> InputHealth;

            [ReadOnly]
            public float Delta;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var shots = batchInChunk.GetNativeArray(InputShot);
                var entities = batchInChunk.GetNativeArray(InputEntity);
                var healths = batchInChunk.GetNativeArray(InputHealth);
                var damages = batchInChunk.GetNativeArray(InputDamage); 
                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    StateShotDone iter = shots[i];
                    Health health = healths[i];
                    DamageSimple damage = damages[i];
                    try
                    {
                        iter.Time += Delta;
                        if (iter.Time < iter.Delay)
                            continue;

                        //TODO: Вынести в WeaponManager
                        health.Value -= damages[i].Def.Link.Value;

                        damage.Def.Link.RemoveComponentData(entities[i], Writer, batchIndex);
                        Writer.RemoveComponent<StateShotDone>(batchIndex, entities[i]);
                        Writer.AddComponent<StateCalcProperty>(batchIndex, entities[i]);
                    }
                    finally
                    {
                        damages[i] = damage;
                        healths[i] = health;
                        shots[i] = iter;
                    }
                }
            }
        }



        [NotBurstCompatible]
        protected override void OnUpdate()
        {
            var job = new ProduceDamageJob()
            {
                InputShot = GetComponentTypeHandle<StateShotDone>(false),
                InputDamage = GetComponentTypeHandle<DamageSimple>(false),
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