using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace Game.Model.Logics
{
    using System.Threading.Tasks;

    using Core;

    using Game.Model.World;

    using Skills;
    using Unity.Burst;
    using Unity.Transforms;

    public partial class EnemySquadLogicDef
    {
        private static readonly int2[] m_Pos = new int2[] { new int2(0, 0), new int2(-2, 0), new int2(2, 0), new int2(-1, -1), new int2(-1, 1), new int2(1, -1), new int2(1, 1) };
        
        public struct InitSquadJob : ILogicJob
        {
            public float Weight => 1;

            [ReadOnly] private ComponentTypeHandle<Teams> m_TeamsHandle;
            [ReadOnly] private ComponentTypeHandle<Squad.Data> m_SquadHandle;
            [ReadOnly] private ComponentDataFromEntity<Move.Moving> m_MoveHandle;

            public InitSquadJob(LogicSystem system)
            {
                m_TeamsHandle = system.GetComponentTypeHandle<Teams>(true);
                m_SquadHandle = system.GetComponentTypeHandle<Squad.Data>(true);
                m_MoveHandle = system.GetComponentDataFromEntity<Move.Moving>(true);
            }

            public void Execute(ExecuteContext context, FunctionPointer<StateCallback> callback)
            {
                var teams = context.GetData(m_TeamsHandle);
                var squad = context.GetData(m_SquadHandle);
                
                NativeArray<Entity> output = new NativeArray<Entity>(squad.Def.Count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                context.Writer.Instantiate(context.SortKey, squad.Def.Prefab.EntityPrefab, output);

                for (var i = 0; i < output.Length; i++)
                {
                    var iter = output[i];

                    context.Writer.AppendToBuffer(context.SortKey, context.Entity, new Squad.UnitPosition { Position = m_Pos[i] });
                    context.Writer.AppendToBuffer(context.SortKey, context.Entity, new Squad.UnitLink { Unit = iter });

                    context.Writer.AddComponent<Teams>(context.SortKey, iter, teams);
                    context.Writer.AddComponent<StateInit>(context.SortKey, iter);
                    context.Writer.AddComponent<Squad.Unit>(context.SortKey, iter, new Squad.Unit { Squad = context.Entity, Index = i });
                }
                output.Dispose();
                
                Move.Place(context.Entity, m_MoveHandle[context.Entity].Def.Link.InitPosition, callback);
                //callback.Invoke(context.Entity, JobResult.Done);
            }
        }

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

            public void Execute(ExecuteContext context, FunctionPointer<StateCallback> callback)
            {
                try
                {
                    var squad = context.GetData<Squad.Unit>(m_SquadSelfHandle);
                    int2 position;
                    var squadPosition = m_MoveHandle[squad.Squad].Def.Link.InitPosition;
                    position = m_SquadPositionHandle[squad.Squad][squad.Index].Position;
                    position += squadPosition;
                    Move.Place(context.Entity, position, callback);
                }
                catch(Exception e)
                {
                    UnityEngine.Debug.LogError($"{e}: {context.Entity}, {context.m_Index}");
                }
            }
        }

        public struct FindPathUnitsJob : ILogicJob
        {
            public float Weight => 1;

            [ReadOnly] private ComponentTypeHandle<Squad.Unit> m_SquadSelfHandle;
            [ReadOnly] private BufferFromEntity<Squad.UnitPosition> m_SquadPositionHandle;
            [ReadOnly] private ComponentDataFromEntity<Move.Moving> m_MoveHandle;
            [ReadOnly] private ComponentDataFromEntity<Translation> m_TranslationHandle;
            [ReadOnly] private Map.Data m_Map;
            private ComponentTypeHandle<EnemyLogic.WorkTime> m_EnemyLogicHandle;

            public FindPathUnitsJob(LogicSystem system)
            {
                m_SquadSelfHandle = system.GetComponentTypeHandle<Squad.Unit>(true);
                m_SquadPositionHandle = system.GetBufferFromEntity<Squad.UnitPosition>(true);
                m_MoveHandle = system.GetComponentDataFromEntity<Move.Moving>(true);
                m_TranslationHandle = system.GetComponentDataFromEntity<Translation>(true);
                m_EnemyLogicHandle = system.GetComponentTypeHandle<EnemyLogic.WorkTime>(false);

                m_Map = Map.Singleton;
            }

            public void Execute(ExecuteContext context, FunctionPointer<StateCallback> callback)
            {
                var squad = context.GetData<Squad.Unit>(m_SquadSelfHandle);
                var logic = context.GetData(m_EnemyLogicHandle);
                if (m_MoveHandle[squad.Squad].State != Move.Moving.InternalState.MoveToPoint)
                {
                    logic.Time += context.Delta;
                    context.SetData(m_EnemyLogicHandle, ref logic);
                    if (logic.Time < .2f)
                    {
                        Task.Delay(100);
                        callback.Invoke(context.Entity, JobResult.Error);
                        return;
                    }
                }
                logic.Time = 0;
                context.SetData(m_EnemyLogicHandle, ref logic);
                
                var squadPosition = m_Map.WordToMap(m_TranslationHandle[squad.Squad].Value);
                var position = m_SquadPositionHandle[squad.Squad][squad.Index].Position;
                position += squadPosition;

                Move.FindPath(context.Entity, position, callback);
            }
        }
    }
}