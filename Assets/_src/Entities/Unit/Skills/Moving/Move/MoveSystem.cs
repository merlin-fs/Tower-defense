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

        //[DisableAutoCreation]
        [UpdateInGroup(typeof(GameLogicSystemGroup))]
        public class System : StateSystem<Moving>
        {
            private EntityQuery m_Query;
            private EntityQuery m_MapQuery;
            private Unity.Mathematics.Random m_Random;

            protected override void OnCreate()
            {
                base.OnCreate();
                m_CommandBuffer = World.GetOrCreateSystem<GameLogicCommandBufferSystem>();
                m_Query = GetEntityQuery(
                    ComponentType.ReadWrite<Commands>()
                );

                m_Query.AddChangedVersionFilter(ComponentType.ReadWrite<Commands>());

                m_MapQuery = GetEntityQuery(
                    ComponentType.ReadOnly<Map>()
                );
                RequireForUpdate(m_Query);

                m_Random = new Unity.Mathematics.Random(847568);
                Service = this;
            }


            struct NewPositionJob : IJobEntityBatch
            {
                [ReadOnly] public Map Map;
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
                                    Writer.SetComponent(batchIndex, entity, data);
                                    Writer.SetComponent(batchIndex, entity, cmd);
                                }
                                finally
                                {
                                    translations[i] = new Translation() { Value = translation.Value };
                                    rotations[i] = new Rotation() { Value = rotation.Value };
                                }
                            }
                            break;
                                
                            case State.FindPath:
                            {
                                data.TargetPosition = cmd.TargetPosition;
                                data.PathPrecent = 0;
                                var info = infos[i];
                                cmd.Value = FindPath(Map, entities[i], ref data, ref info, points[i], times[i])
                                    ? State.MoveToPoint
                                    : State.Error;
                                infos[i] = info;

                                Writer.SetComponent(batchIndex, entity, data);
                                Writer.SetComponent(batchIndex, entity, cmd);
                            }
                            break;
                            case State.MoveToPoint:
                            {
                                var translation = translations[i];
                                var rotation = rotations[i];
                                try
                                {
                                    if (MoveToPoint(Delta, ref data, infos[i], points[i], times[i], ref translation, ref rotation))
                                        cmd.Value = State.Done;

                                    Writer.SetComponent(batchIndex, entity, data);
                                    Writer.SetComponent(batchIndex, entity, cmd);
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
                            break;
                            case State.Done:
                            {
                                cmd.Value = State.None;
                                Writer.SetComponent(batchIndex, entity, cmd);
                                cmd.Callback.Invoke(ref Writer, ref entity, JobResult.Done, batchIndex);
                            }
                            break;
                            case State.Error:
                            {
                                cmd.Value = State.None;
                                Writer.SetComponent(batchIndex, entity, cmd);
                                cmd.Callback.Invoke(ref Writer, ref entity, JobResult.Error, batchIndex);
                            }
                            break;
                        }
                    }
                }
            }

            protected override void OnUpdate()
            {
                var map = m_MapQuery.GetSingleton<Map>();

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

                    InputPathInfo = GetComponentTypeHandle<Map.Path.Info> (false),
                    InputPoints = GetBufferTypeHandle<Map.Path.Points>(false),
                    InputTimes = GetBufferTypeHandle<Map.Path.Times>(false),

                    InputTranslation = GetComponentTypeHandle<Translation>(false),
                    InputRotation = GetComponentTypeHandle<Rotation>(false),
                };

                Dependency = job.ScheduleParallel(m_Query, Dependency);
                m_CommandBuffer.AddJobHandleForProducer(Dependency); 
            }
        }
    } 
}
