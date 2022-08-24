using System;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;

namespace Game.Model.Units.Skills
{
    using Core;
    using World;

    public partial class Move
    {
        public static System Service;

        public static void Place(Entity entity, int2 value, FunctionPointer<StateCallback> callback, int sortKey)
        {
            Service.Writer.SetComponent(sortKey, entity, new Commands() { Value = State.Init, TargetPosition = value, Callback = callback });
        }

        public static void MoveTo(Entity entity, int2 value, FunctionPointer<StateCallback> callback, int sortKey)
        {
            Service.Writer.SetComponent(sortKey, entity, new Commands() { Value = State.FindPath, TargetPosition = value, Callback = callback });
        }

        [UpdateInGroup(typeof(GameLogicSystemGroup))]
        public class System : StateSystem<Moving>
        {
            private EntityQuery m_Query;
            private Unity.Mathematics.Random m_Random;

            protected override void OnCreate()
            {
                base.OnCreate();
                m_CommandBuffer = World.GetOrCreateSystem<GameLogicCommandBufferSystem>();
                m_Query = GetEntityQuery(
                    ComponentType.ReadWrite<Commands>()
                );

                m_Query.AddChangedVersionFilter(ComponentType.ReadWrite<Commands>());
                RequireForUpdate(m_Query);

                m_Random = new Unity.Mathematics.Random(847568);
                Service = this;
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

                public ComponentTypeHandle<Translation> InputTranslation;
                public ComponentTypeHandle<Rotation> InputRotation;

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
                    var rotations = batchInChunk.GetNativeArray(InputRotation);


                    for (var i = 0; i < batchInChunk.Count; i++)
                    {
                        var data = datas[i];
                        var cmd = commands[i];
                        var entity = entities[i];


                        switch (cmd.Value)
                        {
                            case State.Init:
                            {
                                var translation = translations[i];
                                var rotation = rotations[i];
                                try
                                {
                                    data.TargetPosition = cmd.TargetPosition;
                                    cmd.Value = SetToPoint(Map, ref data, ref translation, ref rotation)
                                        ? State.Done
                                        : State.Error;
                                    Writer.SetComponent(batchIndex, entity, cmd);
                                }
                                finally
                                {
                                    datas[i] = data;
                                    translations[i] = new Translation() { Value = translation.Value };
                                    rotations[i] = new Rotation() { Value = rotation.Value };
                                }
                            }
                            break;

                            case State.FindPath:
                            {
                                cmd.Value = State.None;
                                data.TargetPosition = cmd.TargetPosition;
                                datas[i] = data;
                                Writer.SetComponent(batchIndex, entity, cmd);
                                FindPath(Map, entities[i], data, 
                                    (path) =>
                                    {
                                        var buff = Service.Writer.SetBuffer<Map.Path.Points>(0, entity);
                                        buff.ResizeUninitialized(path.Length);
                                        Parallel.For(0, path.Length, (i) =>
                                        {
                                            var p = path[i];
                                            buff[i] = new float3(p.x, p.y, 0);

                                        });
                                        cmd.Value = State.FindPathDone;
                                        Service.Writer.SetComponent(batchIndex, entity, cmd);
                                        path.Dispose();
                                    });
                            }
                            break;
                            case State.FindPathDone:
                            {
                                data.TargetPosition = cmd.TargetPosition;
                                data.PathPrecent = 0;
                                var info = infos[i];
                                cmd.Value = FindPath(Map, ref data, ref info, points[i], times[i])
                                    ? State.MoveToPoint
                                    : State.Error;
                                infos[i] = info;
                                datas[i] = data;
                                Writer.SetComponent(batchIndex, entity, cmd);
                            }
                            break;
                            case State.Done:
                            {
                                cmd.Value = State.None;
                                Writer.SetComponent(batchIndex, entity, cmd);
                                cmd.Callback.Invoke(Writer, entity, JobResult.Done, batchIndex);
                            }
                            break;
                            case State.Error:
                            {
                                cmd.Value = State.None;
                                Writer.SetComponent(batchIndex, entity, cmd);
                                cmd.Callback.Invoke(Writer, entity, JobResult.Error, batchIndex);
                            }
                            break;
                        }
                    }
                }
            }

            protected override void OnUpdate()
            {

                NativeArray<Entity> limitToEntityArray = m_Query.ToEntityArray(Allocator.TempJob);

                var map = Map.Singleton;

                var job = new NewPositionJob()
                {
                    Map = map,
                    LastSystemVersion = LastSystemVersion,
                    Delta = Time.DeltaTime,

                    Random = m_Random,

                    Writer = Writer,

                    InputEntity = GetEntityTypeHandle(),

                    InputCommand = GetComponentTypeHandle<Commands>(false),
                    InputData = GetComponentTypeHandle<Moving>(false),

                    InputPathInfo = GetComponentTypeHandle<Map.Path.Info>(false),
                    InputPoints = GetBufferTypeHandle<Map.Path.Points>(false),
                    InputTimes = GetBufferTypeHandle<Map.Path.Times>(false),

                    InputTranslation = GetComponentTypeHandle<Translation>(false),
                    InputRotation = GetComponentTypeHandle<Rotation>(false),
                };

                Dependency = job.ScheduleParallel(m_Query, ScheduleGranularity.Entity, limitToEntityArray, Dependency);
                limitToEntityArray.Dispose(Dependency);
                //Dependency = job.ScheduleParallel(m_Query, 1, Dependency);
                //m_CommandBuffer.AddJobHandleForProducer(Dependency);
            }
        }

        //[DisableAutoCreation]
        [UpdateInGroup(typeof(GameLogicSystemGroup))]
        public partial class MoveToSystem : SystemBase
        {
            private EntityQuery m_Query;
            protected EntityCommandBufferSystem m_CommandBuffer;
            protected override void OnCreate()
            {
                base.OnCreate();
                m_CommandBuffer = World.GetOrCreateSystem<GameLogicCommandBufferSystem>();

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
                [ReadOnly] public ComponentTypeHandle<Commands> InputCommand;
                [ReadOnly] public ComponentTypeHandle<Map.Path.Info> InputPathInfo;
                [ReadOnly] public BufferTypeHandle<Map.Path.Points> InputPoints;
                [ReadOnly] public BufferTypeHandle<Map.Path.Times> InputTimes;

                public EntityCommandBuffer.ParallelWriter Writer;

                public ComponentTypeHandle<Moving> InputData;

                public ComponentTypeHandle<Translation> InputTranslation;
                public ComponentTypeHandle<Rotation> InputRotation;

                public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
                {
                    var commands = batchInChunk.GetNativeArray(InputCommand);
                    var datas = batchInChunk.GetNativeArray(InputData);
                    var infos = batchInChunk.GetNativeArray(InputPathInfo);
                    var points = batchInChunk.GetBufferAccessor(InputPoints);
                    var times = batchInChunk.GetBufferAccessor(InputTimes);
                    var entities = batchInChunk.GetNativeArray(InputEntity);
                    var translations = batchInChunk.GetNativeArray(InputTranslation);
                    var rotations = batchInChunk.GetNativeArray(InputRotation);


                    for (var i = 0; i < batchInChunk.Count; i++)
                    {
                        var data = datas[i];
                        var cmd = commands[i];
                        var entity = entities[i];

                        if (cmd.Value != State.MoveToPoint)
                            continue;

                        var translation = translations[i];
                        var rotation = rotations[i];
                        try
                        {
                            if (MoveToPoint(Delta, ref data, infos[i], points[i], times[i], ref translation, ref rotation))
                            {
                                cmd.Value = State.Done;
                                Writer.SetComponent(batchIndex, entity, cmd);
                            }
                            datas[i] = data;
                        }
                        finally
                        {
                            var diff = translation.Value - translations[i].Value;
                            if (math.length(diff) < 0)
                            {
                            }
                            translations[i] = new Translation() { Value = translation.Value };
                            rotations[i] = new Rotation() { Value = rotation.Value };
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
                    Delta = Time.DeltaTime,

                    Writer = m_CommandBuffer.CreateCommandBuffer().AsParallelWriter(),

                    InputEntity = GetEntityTypeHandle(),

                    InputCommand = GetComponentTypeHandle<Commands>(true),
                    InputData = GetComponentTypeHandle<Moving>(false),

                    InputPathInfo = GetComponentTypeHandle<Map.Path.Info>(true),
                    InputPoints = GetBufferTypeHandle<Map.Path.Points>(true),
                    InputTimes = GetBufferTypeHandle<Map.Path.Times>(true),

                    InputTranslation = GetComponentTypeHandle<Translation>(false),
                    InputRotation = GetComponentTypeHandle<Rotation>(false),
                };

                Dependency = job.ScheduleParallel(m_Query, Dependency);
            }
        }
    }
}
