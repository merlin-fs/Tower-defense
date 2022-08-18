using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

namespace Game.Model.Units
{
    using World;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    public partial class BusyAddTilesSystem : SystemBase
    {
        private EntityQuery m_Query;
        private EntityQuery m_QueryMap;

        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(
                ComponentType.ReadOnly<StateInit>(),
                ComponentType.ReadOnly<SetPositionOnMap>()
            );

            m_QueryMap = GetEntityQuery(
                ComponentType.ReadWrite<Map>()
            );
            RequireForUpdate(m_Query);
        }


        struct InitPositionJob : IJobEntityBatch
        {
            [ReadOnly]
            public EntityTypeHandle InputEntity;
            [ReadOnly]
            public ComponentTypeHandle<SetPositionOnMap> InputPosition;
            [ReadOnly]
            public Map Map;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var positions = batchInChunk.GetNativeArray(InputPosition);
                var entities = batchInChunk.GetNativeArray(InputEntity);

                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    Map.Tiles.AddEntity(positions[i].TargetPosition, entities[i]);
                }
            }
        }

        protected override void OnUpdate()
        {
            var job = new InitPositionJob()
            {
                Map = m_QueryMap.GetSingleton<Map>(),
                InputEntity = GetEntityTypeHandle(),
                InputPosition = GetComponentTypeHandle<SetPositionOnMap>(true),
            };
            Dependency = job.ScheduleParallel(m_Query, Dependency);
        }
    }

    [UpdateInGroup(typeof(GameDoneSystemGroup))]
    public partial class BusyRemoveTilesSystem : SystemBase
    {
        private EntityQuery m_Query;
        private EntityQuery m_QueryMap;

        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(
                ComponentType.ReadOnly<StateDead>(),
                ComponentType.ReadOnly<SetPositionOnMap>()
            );

            m_QueryMap = GetEntityQuery(
                ComponentType.ReadWrite<Map>()
            );
            RequireForUpdate(m_Query);
        }


        struct InitPositionJob : IJobEntityBatch
        {
            [ReadOnly]
            public ComponentTypeHandle<SetPositionOnMap> InputPosition;
            [ReadOnly]
            public Map Map;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var positions = batchInChunk.GetNativeArray(InputPosition);

                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    Map.Tiles.DelEntity(positions[i].TargetPosition);
                }
            }
        }

        protected override void OnUpdate()
        {
            var job = new InitPositionJob()
            {
                Map = m_QueryMap.GetSingleton<Map>(),
                InputPosition = GetComponentTypeHandle<SetPositionOnMap>(true),
            };
            Dependency = job.ScheduleParallel(m_Query, Dependency);
        }
    }

}