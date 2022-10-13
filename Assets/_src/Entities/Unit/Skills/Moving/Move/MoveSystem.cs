using System;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;

namespace Game.Model.Skills
{
    using Core;
    using Unity.Jobs;
    using World;
    using static Unity.Burst.Intrinsics.X86.Avx;

    public partial class Move
    {
        public static System Instance { get; private set; }

        public static void MoveTo(Entity entity, FunctionPointer<StateCallback> callback)
        {
            Instance.SendData(entity, new Commands() { Value = Command.MoveToPoint, Callback = callback });
        }

        public static void FindPath(Map.Data map, Entity entity, ref Moving moving, Action<NativeArray<int2>> callback)
        {
            FindPath(map, entity, moving, callback);
        }

        public static void SetPath(Map.Data map, Entity entity, ref Moving moving, Action<NativeArray<int2>> callback)
        {
            FindPath(map, entity, moving, callback);
        }

        public static void FindAndSetPath(Map.Data map, Entity entity, Moving moving, FunctionPointer<StateCallback> callback)
        {
            FindPath(map, entity, moving, path =>
            {
                var cmd = new Commands
                {
                    Callback = callback,
                    Value = Command.SetPath,
                };
                cmd.Path.Length = path.Length;
                Parallel.For(0, path.Length, (i) =>
                {
                    cmd.Path[i] = path[i];
                });
                Instance.SendData(entity, cmd);
            });
        }

        [UpdateInGroup(typeof(GameLogicSystemGroup))]
        public partial class System : SystemBase
        {
            private EntityQuery m_Query;
            private Unity.Mathematics.Random m_Random;
            private EntityCommandBufferSystem m_CommandBuffer;
            
            public EntityCommandBuffer NewBuffer => m_CommandBuffer.CreateCommandBuffer();

            public void SendData(Entity entity, Commands value)
            {
                UnityEngine.Debug.Log($"SendData: {entity}: {value.Value}");
                m_CommandBuffer.CreateCommandBuffer().SetComponent(entity, value);
                /*
                lock (m_Lock)
                    m_Queue.Enqueue(new QueueItem { Entity = entity, Value = value });
                */
            }

            private struct QueueItem
            {
                public Commands Value;
                public Entity Entity;
            }

            private NativeQueue<QueueItem> m_Queue;
            private readonly object m_Lock = new object();

            protected override void OnCreate()
            {
                base.OnCreate();
                Instance = this;
                m_Queue = new NativeQueue<QueueItem>(Allocator.Persistent);

                m_CommandBuffer = World.GetOrCreateSystem<GameLogicCommandBufferSystem>();
                m_Query = GetEntityQuery(
                    ComponentType.ReadWrite<Commands>()
                );

                m_Query.AddChangedVersionFilter(ComponentType.ReadWrite<Commands>());
                RequireForUpdate(m_Query);

                m_Random = new Unity.Mathematics.Random(847568);
            }

            protected override void OnDestroy()
            {
                m_Queue.Dispose();
                base.OnDestroy();
            }

            struct NewPositionJob : IJobEntityBatch
            {
                [ReadOnly] public Map.Data Map;
                [ReadOnly] public uint LastSystemVersion;
                [ReadOnly] public float Delta;
                [ReadOnly] public EntityTypeHandle InputEntity;
                public Unity.Mathematics.Random Random;

                public EntityCommandBuffer.ParallelWriter Writer;

                public ComponentTypeHandle<Commands> InputCommand;
                public ComponentTypeHandle<Moving> InputData;
                public ComponentTypeHandle<Map.Path.Info> InputPathInfo;
                public BufferTypeHandle<Map.Path.Points> InputPoints;
                public BufferTypeHandle<Map.Path.Times> InputTimes;

                public ComponentTypeHandle<LocalToWorldTransform> InputTranslation;

                public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
                {
                    var change = batchInChunk.DidChange(InputCommand, LastSystemVersion);
                    if (!change)
                        return;

                    var commands = batchInChunk.GetNativeArray(InputCommand);
                    var datas = batchInChunk.GetNativeArray(InputData);
                    var infos = batchInChunk.GetNativeArray(InputPathInfo);
                    var points = batchInChunk.GetBufferAccessor(InputPoints);
                    var times = batchInChunk.GetBufferAccessor(InputTimes);
                    var entities = batchInChunk.GetNativeArray(InputEntity);
                    var translations = batchInChunk.GetNativeArray(InputTranslation);

                    for (var i = 0; i < batchInChunk.Count; i++)
                    {
                        var data = datas[i];
                        var cmd = commands[i];
                        var entity = entities[i];
                        
                        switch (cmd.Value)
                        {
                            case Command.MoveToPoint:
                            {
                                if (data.State != Moving.InternalState.None)
                                    continue;
                               data.State = Moving.InternalState.MoveToPoint;
                            }
                            break;

                            case Command.SetPath:
                            {
                                data.State = Moving.InternalState.None;
                                data.TargetPosition = cmd.TargetPosition;
                                data.PathPrecent = 0;
                                var info = infos[i];
                                
                                var result = FindPath(Map, cmd.Path, ref data, ref info, points[i], times[i])
                                    ? JobResult.Done
                                    : JobResult.Error;
                                infos[i] = info;
                                cmd.Path.Clear();
                                UnityEngine.Debug.Log($"SetPath: {entity}, {cmd.Value}");
                                cmd.Callback.Invoke(entity, result);
                            }
                            break;
                        }
                        cmd.Value = Command.None;
                        datas[i] = data;
                        commands[i] = cmd;
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

            protected override void OnUpdate()
            {
                /*
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
                */

                var map = Map.Singleton;
                var job = new NewPositionJob()
                {
                    Map = map,
                    LastSystemVersion = LastSystemVersion,
                    Delta = SystemAPI.Time.DeltaTime,

                    Random = m_Random,

                    Writer = m_CommandBuffer.CreateCommandBuffer().AsParallelWriter(),

                    InputEntity = GetEntityTypeHandle(),

                    InputCommand = GetComponentTypeHandle<Commands>(false),
                    InputData = GetComponentTypeHandle<Moving>(false),

                    InputPathInfo = GetComponentTypeHandle<Map.Path.Info>(false),
                    InputPoints = GetBufferTypeHandle<Map.Path.Points>(false),
                    InputTimes = GetBufferTypeHandle<Map.Path.Times>(false),

                    InputTranslation = GetComponentTypeHandle<LocalToWorldTransform>(false),
                };
                NativeArray<Entity> limitToEntityArray = m_Query.ToEntityArray(Allocator.TempJob);
                Dependency = job.ScheduleParallel(m_Query, ScheduleGranularity.Entity, limitToEntityArray, Dependency);
                limitToEntityArray.Dispose(Dependency);
                //Dependency = job.ScheduleParallel(m_Query, Dependency);
                //Dependency = job.ScheduleParallel(m_Query, 1, Dependency);
                m_CommandBuffer.AddJobHandleForProducer(Dependency);
            }
        }

        //[DisableAutoCreation]
        [UpdateInGroup(typeof(GameLogicSystemGroup))]
        public partial class MoveToSystem : SystemBase
        {
            private EntityQuery m_Query;

            protected override void OnCreate()
            {
                base.OnCreate();
                m_Query = GetEntityQuery(
                    ComponentType.ReadOnly<Commands>()
                );
                RequireForUpdate(m_Query);
            }

            struct MoveToJob : IJobEntityBatch
            {
                [ReadOnly] public Map.Data Map;
                [ReadOnly] public float Delta;
                [ReadOnly] public EntityTypeHandle InputEntity;
                [ReadOnly] public ComponentTypeHandle<Map.Path.Info> InputPathInfo;
                [ReadOnly] public BufferTypeHandle<Map.Path.Points> InputPoints;
                [ReadOnly] public BufferTypeHandle<Map.Path.Times> InputTimes;

                public ComponentTypeHandle<Commands> InputCommand;
                public ComponentTypeHandle<Moving> InputData;

                public ComponentTypeHandle<LocalToWorldTransform> InputTranslation;

                public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
                {
                    var commands = batchInChunk.GetNativeArray(InputCommand);
                    var datas = batchInChunk.GetNativeArray(InputData);
                    var infos = batchInChunk.GetNativeArray(InputPathInfo);
                    var points = batchInChunk.GetBufferAccessor(InputPoints);
                    var times = batchInChunk.GetBufferAccessor(InputTimes);
                    var entities = batchInChunk.GetNativeArray(InputEntity);
                    var translations = batchInChunk.GetNativeArray(InputTranslation);


                    for (var i = 0; i < batchInChunk.Count; i++)
                    {
                        var data = datas[i];
                        var cmd = commands[i];
                        var entity = entities[i];

                        if (data.State != Moving.InternalState.MoveToPoint)
                            continue;

                        var translation = translations[i];
                        var rotation = translations[i].Value.Rotation;
                        try
                        {
                            if (MoveToPoint(Delta, ref data, infos[i], points[i], times[i], ref translation))
                            {
                                data.State = Moving.InternalState.None;
                                if (cmd.Callback.IsCreated)
                                    cmd.Callback.Invoke(entity, JobResult.Done);
                            }
                            datas[i] = data;
                        }
                        finally
                        {
                        }
                    }
                }
            }

            protected override void OnUpdate()
            {
                var map = Map.Singleton;

                var job = new MoveToJob()
                {
                    Map = map,
                    Delta = SystemAPI.Time.DeltaTime,

                    InputEntity = GetEntityTypeHandle(),

                    InputCommand = GetComponentTypeHandle<Commands>(false),
                    InputData = GetComponentTypeHandle<Moving>(false),

                    InputPathInfo = GetComponentTypeHandle<Map.Path.Info>(true),
                    InputPoints = GetBufferTypeHandle<Map.Path.Points>(true),
                    InputTimes = GetBufferTypeHandle<Map.Path.Times>(true),

                    InputTranslation = GetComponentTypeHandle<LocalToWorldTransform>(false),
                };

                Dependency = job.ScheduleParallel(m_Query, Dependency);
                //m_CommandBuffer.AddJobHandleForProducer(Dependency);
            }
        }
    }
}
