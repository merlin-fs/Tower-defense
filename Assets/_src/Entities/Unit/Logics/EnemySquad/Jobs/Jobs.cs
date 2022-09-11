using System;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Transforms;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace Game.Model.Logics
{
    using Core;
    using World;
    using Skills;
    using TMPro;

    public partial class EnemySquadLogicDef
    {
        private static readonly int2[] m_Pos = new int2[] { new int2(0, 0), new int2(-2, 0), new int2(2, 0), new int2(-1, -1), new int2(-1, 1), new int2(1, -1), new int2(1, 1) };
        
        public struct InitSquadJob : ILogicJob
        {
            public float Weight => 1;

            [NativeDisableParallelForRestriction]
            [ReadOnly] private ComponentTypeHandle<Teams> m_TeamsHandle;
            [NativeDisableParallelForRestriction]
            [ReadOnly] private ComponentTypeHandle<Squad.Data> m_SquadHandle;
            [NativeDisableParallelForRestriction]
            private ComponentDataFromEntity<Move.Moving> m_MoveHandle;

            public InitSquadJob(LogicSystem system)
            {
                m_TeamsHandle = system.GetComponentTypeHandle<Teams>(true);
                m_SquadHandle = system.GetComponentTypeHandle<Squad.Data>(true);
                m_MoveHandle = system.GetComponentDataFromEntity<Move.Moving>(false);
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

                var moving = m_MoveHandle[context.Entity];
                var pos = moving.Def.Link.InitPosition;
                if (context.Jobs.TryGetJob(InitPlaceJob.ID, out InitPlaceJob place))
                    place.Place(context, pos);
                else
                    callback.Invoke(context.Entity, JobResult.Error);

                callback.Invoke(context.Entity, JobResult.Done);
            }
        }

        public struct PlaceUnitsJob : ILogicJob
        {
            public float Weight => 1;
            [NativeDisableParallelForRestriction]
            [ReadOnly] private ComponentTypeHandle<Squad.Unit> m_SquadSelfHandle;
            [NativeDisableParallelForRestriction]
            [ReadOnly] private BufferFromEntity<Squad.UnitPosition> m_SquadPositionHandle;
            [NativeDisableParallelForRestriction]
            private ComponentDataFromEntity<Move.Moving> m_MoveHandle;

            public PlaceUnitsJob(LogicSystem system)
            {
                m_SquadSelfHandle = system.GetComponentTypeHandle<Squad.Unit>(true);
                m_SquadPositionHandle = system.GetBufferFromEntity<Squad.UnitPosition>(true);
                m_MoveHandle = system.GetComponentDataFromEntity<Move.Moving>(false);
            }

            public void Execute(ExecuteContext context, FunctionPointer<StateCallback> callback)
            {
                var squad = context.GetData<Squad.Unit>(m_SquadSelfHandle);
                if (!m_MoveHandle.TryGetComponent(squad.Squad, out var move))
                {
                    callback.Invoke(context.Entity, JobResult.Error);
                    throw new Exception("Can`t get Move.Moving");
                }

                if (!m_SquadPositionHandle.TryGetBuffer(squad.Squad, out var buff))
                {
                    callback.Invoke(context.Entity, JobResult.Error);
                    throw new Exception("Can`t get Squad.UnitPosition 1");
                }
                    

                try
                {
                    var position = buff[squad.Index].Position;
                    var squadPosition = move.Def.Link.InitPosition;
                    position += squadPosition;

                    if (context.Jobs.TryGetJob(InitPlaceJob.ID, out InitPlaceJob place))
                        place.Place(context, position);
                    else
                        callback.Invoke(context.Entity, JobResult.Error);
                    callback.Invoke(context.Entity, JobResult.Done);
                }
                catch(Exception e)
                {
                    UnityEngine.Debug.LogError($"{context.Entity}, index: {context.m_Index}, {squad.Index} - {e}");
                    UnityEngine.Debug.LogError($"{buff.IsCreated}, {buff.Length}, {buff.ToString()}");
                }
            }
        }


        public struct WaitMoveSquadJob : ILogicJob
        {
            public float Weight => 1;
            [NativeDisableParallelForRestriction]
            [ReadOnly] private ComponentDataFromEntity<Move.Moving> m_MoveHandle;
            [NativeDisableParallelForRestriction]
            [ReadOnly] private ComponentTypeHandle<Squad.Unit> m_SquadSelfHandle;

            public WaitMoveSquadJob(LogicSystem system)
            {
                m_SquadSelfHandle = system.GetComponentTypeHandle<Squad.Unit>(true);
                m_MoveHandle = system.GetComponentDataFromEntity<Move.Moving>(true);
            }
            public void Execute(ExecuteContext context, FunctionPointer<StateCallback> callback)
            {
                var squad = context.GetData<Squad.Unit>(m_SquadSelfHandle);
                var moving = m_MoveHandle[squad.Squad];
                if (moving.State != Move.Moving.InternalState.MoveToPoint)
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(1000);
                        callback.Invoke(context.Entity, JobResult.Error);
                    });
                }
                else
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(100);
                        callback.Invoke(context.Entity, JobResult.Done);
                    });
                }
                    
            }
        }

        public struct FindPathUnitsJob : ILogicJob
        {
            public float Weight => 1;
            
            [NativeDisableParallelForRestriction]
            [ReadOnly] private ComponentTypeHandle<Squad.Unit> m_SquadSelfHandle;
            [NativeDisableParallelForRestriction]
            [ReadOnly] private BufferFromEntity<Squad.UnitPosition> m_SquadPositionHandle;
            [NativeDisableParallelForRestriction]
            [ReadOnly] private ComponentDataFromEntity<Move.Moving> m_MoveHandle;
            [NativeDisableParallelForRestriction]
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
                /*
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
                */
                if (!m_SquadPositionHandle.TryGetBuffer(squad.Squad, out var buff))
                {
                    callback.Invoke(context.Entity, JobResult.Error);
                    throw new Exception("Can`t get Squad.UnitPosition 2");
                }
                    

                if (!m_TranslationHandle.TryGetComponent(squad.Squad, out var translation))
                {
                    callback.Invoke(context.Entity, JobResult.Error);
                    throw new Exception("Can`t get Translation");
                }

                try
                {
                    var squadPosition = m_Map.WordToMap(translation.Value);
                    var position = buff[squad.Index].Position;
                    position += squadPosition;

                    var moving = m_MoveHandle[context.Entity];
                    moving.TargetPosition = position;
                    if (math.any(moving.CurrentPosition != moving.TargetPosition))
                        Move.FindAndSetPath(m_Map, context.Entity, moving, callback);
                    else
                        callback.Invoke(context.Entity, JobResult.Error);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"{context.Entity}, index: {context.m_Index}, {squad.Index} - {e}");
                    UnityEngine.Debug.LogError($"{buff.IsCreated}, {buff.Length}, {buff.ToString()}");
                }
            }
        }
    }
}