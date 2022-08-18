using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Model.Units.Skills
{
    using World;

    [UpdateInGroup(typeof(GameTransformSystemGroup))]
    public partial class PathMovingSystem : SystemBase
    {
        private EntityQuery m_Query;
        private static float3 UP = new float3(0f, 1f, 0f);

        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(
                ComponentType.ReadOnly<SetPositionOnMap>(),
                ComponentType.ReadOnly<Map.Path.Points>(),
                ComponentType.ReadOnly<Map.Path.Times>(),
                ComponentType.ReadWrite<Rotation>(),
                ComponentType.ReadWrite<Translation>()

            );

            RequireForUpdate(m_Query);
        }


        struct NewPositionJob : IJobEntityBatch
        {
            public ComponentTypeHandle<SetPositionOnMap> InputPosition;
            public BufferTypeHandle<Map.Path.Points> InputPoints;
            public BufferTypeHandle<Map.Path.Times> InputTimes;

            public ComponentTypeHandle<Translation> InputTranslation;
            public ComponentTypeHandle<Rotation> InputRotation;

            [ReadOnly]
            public float Delta;


            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var translations = batchInChunk.GetNativeArray(InputTranslation);
                var rotations = batchInChunk.GetNativeArray(InputRotation);

                var pathInfos = batchInChunk.GetNativeArray(InputPosition);
                var pathPoints = batchInChunk.GetBufferAccessor(InputPoints);
                var pathTimes = batchInChunk.GetBufferAccessor(InputTimes);

                for (var i = 0; i < translations.Length; i++)
                {
                    var points = pathPoints[i];
                    var times = pathTimes[i];
                    var path = pathInfos[i];
                    if (points.Length == 0 || path.PathPrecent >= 1f)
                        continue;

                    try
                    {
                        float speed = path.Def.Link.Speed * 0.001f;
                        path.PathPrecent += Delta * speed;

                        float time = Map.Path.ConvertToConstantPathTime(path.PathPrecent, path.PathLength, times.AsNativeArray());
                        float3 position = Map.Path.GetPosition(time, false, points.AsNativeArray(), path.PathDeltaTime);
                        quaternion rotation = quaternion.LookRotation(math.normalize(position - translations[i].Value), UP);

                        rotations[i] = new Rotation { Value = rotation, };
                        translations[i] = new Translation { Value = position, };
                    }
                    finally
                    {
                        pathInfos[i] = path;
                    }
                }
            }
        }


        protected override void OnUpdate()
        {
            var job = new NewPositionJob()
            {
                Delta = Time.DeltaTime,
                InputPosition = GetComponentTypeHandle<SetPositionOnMap>(false),
                InputPoints = GetBufferTypeHandle<Map.Path.Points>(true),
                InputTimes = GetBufferTypeHandle<Map.Path.Times>(true),

                InputTranslation = GetComponentTypeHandle<Translation>(),
                InputRotation = GetComponentTypeHandle<Rotation>(),
            };
            Dependency = job.ScheduleParallel(m_Query, Dependency);
        }
    }
}