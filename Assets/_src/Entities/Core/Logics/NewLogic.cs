using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Common.Defs;

namespace Game.Model.Logics
{
    using Common.Core;
    using Core;

    public struct NewLogic : ILogic
    {
        private int m_CurrentState;
        private JobState m_JobState;
        public int CurrentState { get => m_CurrentState; set => m_CurrentState = value; }
        public JobState Value { get => m_JobState; set => m_JobState = value; }


        private ReferenceObject<ILogicDef> m_Def;
        public NewLogic(ReferenceObject<ILogicDef> def)
        {
            m_Def = def;
            m_CurrentState = 0;
            m_JobState = JobState.None;
        }

        public ILogicState GetNextTransition(int current)
        {
            return null;
            /*
            IEnumerable<ILogicJob> list = jobs.GetEnterTransition();
            if (value != 0)
                list = jobs.GetTransition(value, jobResult);
            var result = Random(list);
            return Logic.GetID(result);
            */
        }
    }

    public partial class LogicSystem<T> : SystemBase
        where T : struct, ILogic
    {
        private EntityQuery m_Query;
        private EntityCommandBufferSystem m_CommandBuffer;
        protected override void OnCreate()
        {
            base.OnCreate();
            m_CommandBuffer = World.GetOrCreateSystem<GameLogicCommandBufferSystem>();

            m_Query = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadWrite<T>() },
                Options = EntityQueryOptions.IncludeDisabled
            });
            m_Query.AddChangedVersionFilter(ComponentType.ReadWrite<T>());
            RequireForUpdate(m_Query);
        }

        struct LogicJob : IJobEntityBatch
        {
            [ReadOnly] public EntityTypeHandle InputEntity;
            public ComponentTypeHandle<T> InputLogic;
            public EntityCommandBuffer.ParallelWriter Writer;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var datas = batchInChunk.GetNativeArray(InputLogic);
                var entities = batchInChunk.GetNativeArray(InputEntity);

                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    var data = datas[i];
                    var next = data.GetNextTransition(data.CurrentState);

                    data.CurrentState = next.GetHashCode();
                    datas[i] = data;
                    Writer.AddComponentIData(entities[i], next, batchIndex);
                }
            }
        }

        protected override void OnUpdate()
        {
            var job = new LogicJob()
            {
                InputEntity = GetEntityTypeHandle(),
                InputLogic = GetComponentTypeHandle<T>(),
                Writer = m_CommandBuffer.CreateCommandBuffer().AsParallelWriter(),
            };

            NativeArray<Entity> limitToEntityArray = m_Query.ToEntityArray(Allocator.TempJob);
            Dependency = job.ScheduleParallel(m_Query, ScheduleGranularity.Entity, limitToEntityArray, Dependency);
            limitToEntityArray.Dispose(Dependency);
        }
    }


    //[Unity.Entities.WriteGroup()]
    public partial class LogicJobSystem<T, J> : SystemBase
        where T : struct, ILogic
        where J : struct, ILogicPart
    {
        private EntityQuery m_Query;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_Query = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadWrite<T>(), ComponentType.ReadWrite<J>() },
                Options = EntityQueryOptions.IncludeDisabled
            });
            m_Query.AddChangedVersionFilter(ComponentType.ReadWrite<J>());
            RequireForUpdate(m_Query);
        }

        protected override void OnUpdate()
        {
            J job = new J();
            job.Init(this);

            NativeArray<Entity> limitToEntityArray = m_Query.ToEntityArray(Allocator.TempJob);
            Dependency = job.ScheduleParallel(m_Query, ScheduleGranularity.Entity, limitToEntityArray, Dependency);
            limitToEntityArray.Dispose(Dependency);
            //Dependency = job.ScheduleParallel(m_Query, Dependency);
            //m_CommandBuffer.AddJobHandleForProducer(Dependency);
        }
    }
}