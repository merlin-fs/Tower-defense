using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using System.Runtime.InteropServices;

namespace Game.Model.Logics
{
    using Core;
    using static Unity.Burst.Intrinsics.X86.Avx;

    public abstract partial class LogicSystem : SystemBase
    {
        protected EntityCommandBufferSystem m_CommandBuffer;

        public EntityQuery BuildEntityQuery(params ComponentType[] componentTypes)
        {
            return GetEntityQuery(componentTypes);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            m_CommandBuffer = World.GetOrCreateSystem<GameLogicCommandBufferSystem>();
            m_StateMachine = new LogicStateMachine(EntityManager, this, m_CommandBuffer);
        }
    }

    [UpdateInGroup(typeof(GameLogicInitSystemGroup), OrderFirst = true)]
    public partial class LogicSystem<T, S>: LogicSystem
        where T : struct, ILogic
        where S : struct, ILogicState
    {
        protected EntityQuery m_Query;
        private FunctionPointer<StateCallback> m_Callback;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_CommandBuffer = World.GetOrCreateSystem<GameLogicCommandBufferSystem>();

            m_Query = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadWrite<T>(), ComponentType.ReadWrite<S>() },
                Options = EntityQueryOptions.IncludeDisabled
            });
            m_Query.AddChangedVersionFilter(ComponentType.ReadWrite<S>());
            RequireForUpdate(m_Query);

            m_Callback = new FunctionPointer<StateCallback>(Marshal.GetFunctionPointerForDelegate<StateCallback>(SetResult));
            //JobsUtility.JobDebuggerEnabled = false;
        }

        static void SetResult(EntityCommandBuffer.ParallelWriter writer, Entity entity, JobResult result, int sortKey)
        {
            var state = result == JobResult.Error ? JobState.Error : JobState.None;
            SetState(writer, entity, state, sortKey);
        }

        static void SetState(EntityCommandBuffer.ParallelWriter writer, Entity entity, JobState state, int sortKey)
        {
            //UnityEngine.Debug.Log($"result: {entity}: {state}");
            new S().SetState(writer, entity, state, sortKey);
        }

        public struct LogicJob : IJobEntityBatch
        {
            [ReadOnly] public uint LastSystemVersion;
            [ReadOnly] public EntityTypeHandle InputEntity;
            public FunctionPointer<StateCallback> Callback;

            public ComponentTypeHandle<T> InputLogic;
            public ComponentTypeHandle<S> InputState;

            public EntityCommandBuffer.ParallelWriter Writer;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var change = batchInChunk.DidChange(InputState, LastSystemVersion);
                if (!change)
                    return;

                var entities = batchInChunk.GetNativeArray(InputEntity);
                var datas = batchInChunk.GetNativeArray(InputLogic);
                var states = batchInChunk.GetNativeArray(InputState);

                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    var state = states[i];
                    if (state.Value == JobState.Running)
                        continue;

                    var iter = datas[i];
                    var entity = entities[i];
                    var result = state.Value == JobState.Error ? JobResult.Error : JobResult.Done;

                    var next = iter.Def.GetTransition(iter.CurrentJob, result);
                    if (next != 0)
                    {
                        if (iter.Def.Logic.TryGetJob(next, out ILogicPart logicJob))
                        {
                            //UnityEngine.Debug.Log($"{typeof(T)}: {entity}: {logicJob.GetType()}");
                            state.Value = JobState.Running;
                            try
                            {
                                SetState(Writer, entity, state.Value, batchIndex);
                                iter.CurrentJob = next;
                                var context = new ExecuteContext(Writer, batchInChunk, entity, i, Callback, batchIndex);
                                logicJob.Execute(context);
                            }
                            catch
                            {
                                state.Value = JobState.Error;
                                SetState(Writer, entity, state.Value, batchIndex);
                                throw;
                            }
                            finally
                            {
                                datas[i] = iter;
                            }
                        }
                    }
                }
            }
        }

        protected override void OnUpdate()
        {
            var job = new LogicJob
            {
                Writer = m_CommandBuffer.CreateCommandBuffer().AsParallelWriter(),
                LastSystemVersion = LastSystemVersion,
                Callback = m_Callback,
                InputLogic = GetComponentTypeHandle<T>(false),
                InputState = GetComponentTypeHandle<S>(false),
                InputEntity = GetEntityTypeHandle(),
            };

            foreach (var iter in StateMachine.Parts)
                iter.Init(this);

            NativeArray<Entity> limitToEntityArray = m_Query.ToEntityArray(Allocator.TempJob);
            Dependency = job.ScheduleParallel(m_Query, ScheduleGranularity.Entity, limitToEntityArray, Dependency);
            limitToEntityArray.Dispose(Dependency);
            //Dependency = job.ScheduleParallel(m_Query, Dependency);
            //m_CommandBuffer.AddJobHandleForProducer(Dependency);
        }
    }

    public struct ExecuteContext
    {
        public Entity Entity { get; }
        public EntityCommandBuffer.ParallelWriter Writer { get; }
        public int SortKey { get; }
        public FunctionPointer<StateCallback> Callback { get; }

        public int m_Index;
        private ArchetypeChunk m_BatchInChunk;


        public DynamicBuffer<T> GetData<T>(BufferTypeHandle<T> handle)
            where T : struct, IBufferElementData
        {
            return m_BatchInChunk.GetBufferAccessor(handle)[m_Index];
        }

        public T GetData<T>(ComponentTypeHandle<T> handle)
            where T : struct, IComponentData
        {
            return m_BatchInChunk.GetNativeArray(handle)[m_Index];
        }

        public ExecuteContext(EntityCommandBuffer.ParallelWriter writer, ArchetypeChunk batchInChunk, Entity entity, int index, FunctionPointer<StateCallback> callback, int sortKey)
        {
            Writer = writer;
            Entity = entity;
            SortKey = sortKey;
            m_Index = index;
            m_BatchInChunk = batchInChunk;
            Callback = callback;
        }
    }

}