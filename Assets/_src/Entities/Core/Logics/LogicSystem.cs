using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;

namespace Game.Model.Logics
{
    using Core;
    
    public abstract partial class LogicSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            m_StateMachine = new LogicStateMachine();
        }
    }

    [UpdateInGroup(typeof(GameLogicInitSystemGroup), OrderFirst = true)]
    public partial class LogicSystem<T, S>: LogicSystem
        where T : struct, ILogic
        where S : struct, ILogicState
    {
        public static LogicSystem<T, S> Instance { get; private set; }

        public void SendData(Entity entity, S value)
        {
            UnityEngine.Debug.Log($"SendData: {entity}: {value.Value}");
            lock (m_Lock)
                m_Queue.Enqueue(new QueueItem { Entity = entity, Value = value });
        }

        private struct QueueItem
        {
            public S Value;
            public Entity Entity;
        }

        private NativeQueue<QueueItem> m_Queue;
        private readonly object m_Lock = new object();

        private EntityCommandBufferSystem m_CommandBuffer;
        private EntityQuery m_Query;
        private FunctionPointer<StateCallback> m_Callback;

        protected override void OnCreate()
        {
            base.OnCreate();
            Instance = this;

            m_Queue = new NativeQueue<QueueItem>(Allocator.Persistent);

            m_Query = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadWrite<T>(), ComponentType.ReadWrite<S>() },
                Options = EntityQueryOptions.IncludeDisabled
            });
            m_Query.AddChangedVersionFilter(ComponentType.ReadWrite<S>());
            RequireForUpdate(m_Query);

            m_CommandBuffer = World.GetOrCreateSystem<GameLogicCommandBufferSystem>();
            m_Callback = new FunctionPointer<StateCallback>(Marshal.GetFunctionPointerForDelegate<StateCallback>(SetResult));
            //JobsUtility.JobDebuggerEnabled = false;
        }

        protected override void OnDestroy()
        {
            m_Queue.Dispose();
            base.OnDestroy();
        }

        static void SetResult(Entity entity, JobResult result)
        {
            var state = result == JobResult.Error ? JobState.Error : JobState.None;
            SetState(entity, state);
        }

        static void SetState(Entity entity, JobState state)
        {
            Instance.SendData(entity, new S { Value = state });
        }

        public struct LogicJob : IJobEntityBatch
        {
            [ReadOnly] public uint LastSystemVersion;
            [ReadOnly] public EntityTypeHandle InputEntity;
            public FunctionPointer<StateCallback> Callback;

            public ComponentTypeHandle<T> InputLogic;
            public ComponentTypeHandle<S> InputState;

            public EntityCommandBuffer.ParallelWriter Writer;
            public LogicStateMachine.StateJobs Jobs;

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

                    var next = iter.Def.GetTransition(Jobs, iter.CurrentJob, result);
                    if (next != 0)
                    {
                        if (Jobs.TryGetJob(next, out ILogicJob logicJob))
                        {
                            UnityEngine.Debug.Log($"{typeof(T)}: {entity}: {logicJob.GetType()}");
                            state.Value = JobState.Running;
                            try
                            {
                                SetState(entity, state.Value);
                                iter.CurrentJob = next;
                                var context = new ExecuteContext(Writer, batchInChunk, entity, i, Callback, batchIndex);
                                logicJob.Execute(context);
                            }
                            catch
                            {
                                state.Value = JobState.Error;
                                SetState(entity, state.Value);
                                throw;
                            }
                            finally
                            {
                                states[i] = state;
                                datas[i] = iter;
                            }
                        }
                    }
                }
            }
        }

        struct QueueJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter Writer;
            public NativeArray<QueueItem> Items;

            public void Execute(int index)
            {
                var iter = Items[index];
                UnityEngine.Debug.Log($"SetComponent: {iter.Entity}: {iter.Value.Value}");
                Writer.SetComponent(index, iter.Entity, iter.Value);
            }
        }

        protected unsafe override void OnUpdate()
        {
            NativeArray<QueueItem> items;
            lock (m_Lock)
            {
                items = m_Queue.ToArray(Allocator.TempJob);
                m_Queue.Clear();
            }

            var queueJob = new QueueJob
            {
                Writer = m_CommandBuffer.CreateCommandBuffer().AsParallelWriter(),
                Items = items,
            }.Schedule(items.Length, 1);
            items.Dispose(queueJob);
            m_CommandBuffer.AddJobHandleForProducer(queueJob);


            var jobs = StateMachine.PrepareJobs(this);

            var job = new LogicJob
            {
                Writer = m_CommandBuffer.CreateCommandBuffer().AsParallelWriter(),
                Jobs = jobs,

                LastSystemVersion = LastSystemVersion,
                Callback = m_Callback,
                InputLogic = GetComponentTypeHandle<T>(false),
                InputState = GetComponentTypeHandle<S>(false),
                InputEntity = GetEntityTypeHandle(),
            };

            //NativeArray<Entity> limitToEntityArray = m_Query.ToEntityArray(Allocator.TempJob);
            //Dependency = job.ScheduleParallel(m_Query, ScheduleGranularity.Entity, limitToEntityArray, Dependency);
            //limitToEntityArray.Dispose(Dependency);
            Dependency = job.ScheduleParallel(m_Query, Dependency);
            m_CommandBuffer.AddJobHandleForProducer(Dependency);
            jobs.Dispose(Dependency);
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
            var array = m_BatchInChunk.GetNativeArray(handle);
            return array[m_Index];
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