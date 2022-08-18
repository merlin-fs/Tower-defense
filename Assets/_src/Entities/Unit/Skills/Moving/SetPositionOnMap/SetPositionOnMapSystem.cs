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

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(GameTransformSystemGroup))]
    public partial class SetPositionOnMapSystem : SystemBase
    {
        private EntityQuery m_Query;
        private EntityQuery m_MapQuery;

        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(
                ComponentType.ReadOnly<SetPositionOnMap>(),
                ComponentType.ReadWrite<Map.Path.Points>(),
                ComponentType.ReadWrite<Map.Path.Times>()
            );

            m_MapQuery = GetEntityQuery(
                ComponentType.ReadOnly<Map>()
            );
            RequireForUpdate(m_Query);
        }


        struct NewPositionJob : IJobEntityBatch
        {
            [ReadOnly]
            public Map Map;
            [ReadOnly]
            public EntityTypeHandle InputEntity;
            public ComponentTypeHandle<SetPositionOnMap> InputPosition;
            public BufferTypeHandle<Map.Path.Points> InputPoints;
            public BufferTypeHandle<Map.Path.Times> InputTimes;

            [ReadOnly]
            public float Delta;


            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var pathInfos = batchInChunk.GetNativeArray(InputPosition);
                var pathPoints = batchInChunk.GetBufferAccessor(InputPoints);
                var pathTimes = batchInChunk.GetBufferAccessor(InputTimes);
                var entities = batchInChunk.GetNativeArray(InputEntity);

                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    var info = pathInfos[i];
                    try
                    {
                        if (math.any(info.InitPosition != info.TargetPosition))
                        {
                            var path = Map.PathFinder.Execute(Map.GetCostTile, entities[i], info.InitPosition, info.TargetPosition, Map);
                            if (path.Length < 2)
                            {
                                info.InitPosition = info.TargetPosition;
                                path.Dispose();
                                continue;
                            }
                            var points = pathPoints[i];
                            var times = pathTimes[i];

                            points.ResizeUninitialized(path.Length);
                            times.ResizeUninitialized(200);
                            float step = 1f / 200f;

                            for (int j = 0; j < path.Length; j++)
                                points[j] = Map.MapToWord(path[path.Length - (j + 1)]);

                            info.PathDeltaTime = 1f / (points.Length - 1);
                            var pts = points.AsNativeArray();
                            
                            float len = 0f;
                            float3 vector = Map.Path.GetPosition(0f, false, pts, info.PathDeltaTime);

                            for (int j = 1; j < 200; j++)
                            {
                                float pos = step * (float)j;

                                float3 point = Map.Path.GetPosition(pos, false, pts, info.PathDeltaTime);
                                len += math.distance(point, vector);
                                vector = point;
                                times[j - 1] = new Map.Path.Times()
                                {
                                    Time = pos,
                                    Length = len,
                                };
                            };
                            info.PathLength = len;
                            info.InitPosition = info.TargetPosition;
                            path.Dispose();
                        }
                    }
                    finally
                    {
                        pathInfos[i] = info;
                    }
                }
            }
        }

        //[NotBurstCompatible]
        protected override void OnUpdate()
        {
            var map = m_MapQuery.GetSingleton<Map>();

            var job = new NewPositionJob()
            {
                Map = map,
                InputEntity = GetEntityTypeHandle(),
                InputPosition = GetComponentTypeHandle<SetPositionOnMap>(),
                InputPoints = GetBufferTypeHandle<Map.Path.Points>(false),
                InputTimes = GetBufferTypeHandle<Map.Path.Times>(false),

                Delta = Time.DeltaTime,
            };
            Dependency = job.ScheduleParallel(m_Query, Dependency);
        }
    }
}