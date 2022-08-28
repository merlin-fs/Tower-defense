using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

namespace Game.Model
{
    using World;
    using Skills;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    public partial class BusyAddTilesSystem : SystemBase
    {
        private EntityQuery m_Query;

        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(
                ComponentType.ReadOnly<Move.Moving>(),
                ComponentType.ReadOnly<Move.Commands>()
            );
            m_Query.AddChangedVersionFilter(ComponentType.ReadWrite<Move.Commands>());
            RequireForUpdate(m_Query);
        }


        struct InitPositionJob : IJobEntityBatch
        {
            [ReadOnly] public EntityTypeHandle InputEntity;
            [ReadOnly] public ComponentTypeHandle<Move.Moving> InputPosition;
            [ReadOnly] public ComponentTypeHandle<Move.Commands> InputState;
            [ReadOnly] public Map.Data Map;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var positions = batchInChunk.GetNativeArray(InputPosition);
                var entities = batchInChunk.GetNativeArray(InputEntity);
                var commands = batchInChunk.GetNativeArray(InputState);
                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    switch (commands[i].Value)
                    {
                        case Move.State.Init:
                        {
                            Map.Tiles.AddEntity(commands[i].TargetPosition, entities[i]);
                        }
                        break;
                        case Move.State.MoveToPoint:
                        {
                            Map.Tiles.DelEntity(positions[i].CurrentPosition);
                            Map.Tiles.AddEntity(commands[i].TargetPosition, entities[i]);
                        }
                        break;
                    }
                }
            }
        }

        protected override void OnUpdate()
        {
            var job = new InitPositionJob()
            {
                Map = Map.Singleton,
                InputEntity = GetEntityTypeHandle(),
                InputPosition = GetComponentTypeHandle<Move.Moving>(true),
                InputState = GetComponentTypeHandle<Move.Commands>(true),
            };
            Dependency = job.ScheduleParallel(m_Query, Dependency);
        }
    }

    [UpdateInGroup(typeof(GameDoneSystemGroup))]
    public partial class BusyRemoveTilesSystem : SystemBase
    {
        private EntityQuery m_Query;
        
        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(
                ComponentType.ReadOnly<StateDead>(),
                ComponentType.ReadOnly<Move.Moving>()
            );
            RequireForUpdate(m_Query);
        }


        struct InitPositionJob : IJobEntityBatch
        {
            [ReadOnly]
            public ComponentTypeHandle<Move.Moving> InputPosition;
            [ReadOnly]
            public Map.Data Map;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var positions = batchInChunk.GetNativeArray(InputPosition);

                for (var i = 0; i < batchInChunk.Count; i++)
                {
                    Map.Tiles.DelEntity(positions[i].CurrentPosition);
                }
            }
        }

        protected override void OnUpdate()
        {
            var job = new InitPositionJob()
            {
                Map = Map.Singleton,
                InputPosition = GetComponentTypeHandle<Move.Moving>(true),
            };
            Dependency = job.ScheduleParallel(m_Query, Dependency);
        }
    }

}