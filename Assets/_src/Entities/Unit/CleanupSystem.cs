using System;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;


namespace Game.Model.Units
{
    using Skills;

    /*
    [UpdateInGroup(typeof(GameDoneSystemGroup), OrderLast = true)]
    public partial class CleanupSystem : SystemBase
    {
        private EntityQuery m_Query;
        private EntityQuery m_QueryTarget;
        private EntityCommandBufferSystem m_CommandBuffer;

        protected override void OnCreate()
        {
            m_CommandBuffer = World.GetOrCreateSystem<GameDoneSystemCommandBufferSystem>();
            m_Query = GetEntityQuery(
                ComponentType.ReadOnly<StateDead>()
            );

            m_QueryTarget = GetEntityQuery(
                ComponentType.ReadOnly<FindTarget.Target>()
            );
            RequireForUpdate(m_Query);
        }

        struct RemoveTargetJob : IJobEntityBatch
        {
            [DeallocateOnJobCompletion]
            public NativeArray<Entity> RemovedTarget;
            public ComponentTypeHandle<FindTarget.Target> InputTarget;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var targets = batchInChunk.GetNativeArray(InputTarget);
                var removed = RemovedTarget;
                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    Parallel.For(0, RemovedTarget.Length,
                        j =>
                        {
                            if (targets[i].Value == removed[j])
                            {
                                targets[i] = new FindTarget.Target { State = Core.JobState.None, Value = Entity.Null };
                                UnityEngine.Debug.Log($"remove Target");
                            }
                        });
                }
            }
        }

        struct DestroyDeadJob : IJobEntityBatch
        {
            [ReadOnly]
            public EntityTypeHandle InputEntity;
            public EntityCommandBuffer.ParallelWriter Writer;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var entities = batchInChunk.GetNativeArray(InputEntity);
                var write = Writer;


                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    write.RemoveComponent<StateDead>(batchIndex, entities[i]);
                    write.RemoveComponent<StateShot>(batchIndex, entities[i]);
                    write.RemoveComponent<StateCalcProperty>(batchIndex, entities[i]);
                    write.RemoveComponent<StateShotDone>(batchIndex, entities[i]);

                    UnityEngine.Debug.Log($"DestroyEntity");
                    write.DestroyEntity(batchIndex, entities[i]);
                }
            }
        }

        protected override void OnUpdate()
        {
            var removeTargetJob = new RemoveTargetJob()
            {
                RemovedTarget = m_Query.ToEntityArray(Allocator.TempJob),
                InputTarget = GetComponentTypeHandle<FindTarget.Target>(false),
            };
            Dependency = removeTargetJob.ScheduleParallel(m_QueryTarget, Dependency);
            m_CommandBuffer.AddJobHandleForProducer(Dependency);

            var job = new DestroyDeadJob()
            {
                InputEntity = GetEntityTypeHandle(),
                Writer = m_CommandBuffer.CreateCommandBuffer().AsParallelWriter(),
            };
            Dependency = job.ScheduleParallel(m_Query, Dependency);
            m_CommandBuffer.AddJobHandleForProducer(Dependency);
        }
    }
    */
}