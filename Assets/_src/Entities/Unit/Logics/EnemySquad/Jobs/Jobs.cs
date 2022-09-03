using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace Game.Model.Logics
{
    using Core;
    using Skills;

    public partial class EnemySquadLogicDef
    {
        private static int2[] m_Pos = new int2[] { new int2(0, 0), new int2(-2, 0), new int2(2, 0), new int2(-1, -1), new int2(-1, 1), new int2(1, -1), new int2(1, 1) };
        
        public struct InitSquadJob : ILogicJob
        {
            public float Weight => 1;

            [ReadOnly] private ComponentTypeHandle<Teams> m_TeamsHandle;
            [ReadOnly] private ComponentTypeHandle<Squad.Data> m_SquadHandle;
            private BufferTypeHandle<Squad.UnitLink> m_SquadUnits;
            private BufferTypeHandle<Squad.UnitPosition> m_SquadPositions;

            public InitSquadJob(LogicSystem system)
            {
                m_TeamsHandle = system.GetComponentTypeHandle<Teams>(true);
                m_SquadHandle = system.GetComponentTypeHandle<Squad.Data>(true);
                m_SquadUnits = system.GetBufferTypeHandle<Squad.UnitLink>(false);
                m_SquadPositions = system.GetBufferTypeHandle<Squad.UnitPosition>(false);
            }

            public void Execute(ExecuteContext context)
            {
                var teams = context.GetData(m_TeamsHandle);
                var squad = context.GetData(m_SquadHandle);

                var units = context.GetData(m_SquadUnits);
                var positions = context.GetData(m_SquadPositions);
                units.ResizeUninitialized(squad.Def.Count);
                positions.ResizeUninitialized(squad.Def.Count);

                NativeArray<Entity> output = new NativeArray<Entity>(squad.Def.Count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                context.Writer.Instantiate(context.SortKey, squad.Def.Prefab.EntityPrefab, output);

                for (var i = 0; i < output.Length; i++)
                {
                    var iter = output[i];
                    units[i] = new Squad.UnitLink { Unit = iter };
                    positions[i] = new Squad.UnitPosition { Position = m_Pos[i] };

                    context.Writer.AddComponent<Teams>(context.SortKey, iter, teams);
                    context.Writer.AddComponent<StateInit>(context.SortKey, iter);
                    context.Writer.AddComponent<Squad.Unit>(context.SortKey, iter, new Squad.Unit { Squad = context.Entity, Index = i });
                }

                output.Dispose();
                context.Callback.Invoke(context.Entity, JobResult.Done);
            }
        }

        /*
        public class PlaceUnitsTestJob
        {
            public float Weight => 1;

            struct Job
            {
                [ReadOnly] private ComponentTypeHandle<Squad.Unit> m_SquadSelfHandle;
                [ReadOnly] private BufferFromEntity<Squad.UnitPosition> m_SquadPositionHandle;
                [ReadOnly] private ComponentDataFromEntity<Move.Moving> m_MoveHandle;

                public void Init(LogicSystem system)
                {
                    m_SquadSelfHandle = system.GetComponentTypeHandle<Squad.Unit>(true);
                    m_SquadPositionHandle = system.GetBufferFromEntity<Squad.UnitPosition>(true);
                    m_MoveHandle = system.GetComponentDataFromEntity<Move.Moving>(true);
                }

                public void Execute(ExecuteContext context)
                {
                    var squad = context.GetData<Squad.Unit>(m_SquadSelfHandle);
                    int2 position;
                    UnityEngine.Debug.Log($"Get m_MoveHandle 2: {squad.Squad}, {squad.Index}, {context.Entity}, {context.m_Index}");

                    var squadPosition = m_MoveHandle[squad.Squad].Def.Link.InitPosition;
                    position = m_SquadPositionHandle[squad.Squad][squad.Index].Position;
                    position += squadPosition;

                    Move.Place(context.Entity, position, context.Callback);
                }
            }

            public Job Init(LogicSystem system)
            {
                Job job = new Job();
                job.Init(system);
                return job;
            }

        }
        */

        public struct PlaceUnitsJob : ILogicJob
        {
            public float Weight => 1;

            [ReadOnly] private ComponentTypeHandle<Squad.Unit> m_SquadSelfHandle;
            [ReadOnly] private BufferFromEntity<Squad.UnitPosition> m_SquadPositionHandle;
            [ReadOnly] private ComponentDataFromEntity<Move.Moving> m_MoveHandle;

            public PlaceUnitsJob(LogicSystem system)
            {
                m_SquadSelfHandle = system.GetComponentTypeHandle<Squad.Unit>(true);
                m_SquadPositionHandle = system.GetBufferFromEntity<Squad.UnitPosition>(true);
                m_MoveHandle = system.GetComponentDataFromEntity<Move.Moving>(true);
            }

            public void Execute(ExecuteContext context)
            {
                try
                {
                    var squad = context.GetData<Squad.Unit>(m_SquadSelfHandle);
                    int2 position;
                    var squadPosition = m_MoveHandle[squad.Squad].Def.Link.InitPosition;
                    position = m_SquadPositionHandle[squad.Squad][squad.Index].Position;
                    position += squadPosition;
                    Move.Place(context.Entity, position, context.Callback);
                }
                catch(Exception e)
                {
                    UnityEngine.Debug.LogError($"{e}: {context.Entity}, {context.m_Index}");
                }
            }
        }
    }
}