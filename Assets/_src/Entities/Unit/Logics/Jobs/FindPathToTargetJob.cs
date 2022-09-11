using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace Game.Model.Logics
{
    using Core;
    using World;
    using Skills;

    public unsafe struct FindPathToTargetJob : ILogicJob
    {
        public float Weight => 1;

        [ReadOnly] private ComponentDataFromEntity<Translation> m_InputTranslation;
        [ReadOnly] private ComponentDataFromEntity<Teams> m_InputTeams;
        [ReadOnly] private ComponentDataFromEntity<Move.Moving> m_InputMove;

        public FindPathToTargetJob(LogicSystem system)
        {
            m_InputTranslation = system.GetComponentDataFromEntity<Translation>(true);
            m_InputTeams = system.GetComponentDataFromEntity<Teams>(true);
            m_InputMove = system.GetComponentDataFromEntity<Move.Moving>(true);
        }

        public void Execute(ExecuteContext context, FunctionPointer<StateCallback> callback)
        {
            var moving = m_InputMove[context.Entity];
            var enemyTeams = m_InputTeams[context.Entity];
            var map = Map.Singleton;
            var selfRealPosotion = map.MapToWord(moving.CurrentPosition);

            var enemy = FindTarget.FindEnemy(enemyTeams.EnemyTeams, selfRealPosotion, float.MaxValue, m_InputTranslation, m_InputTeams);

            if (enemy != Entity.Null)
            {
                var pos = m_InputMove[enemy].CurrentPosition;
                if (!Map.GeneratePosition(map, ref pos))
                {
                    callback.Invoke(context.Entity, JobResult.Error);
                    return;
                }
                moving.TargetPosition = pos;
                Move.FindAndSetPath(map, context.Entity, moving, callback);
            }
            else
            {
                callback.Invoke(context.Entity, JobResult.Error);
            }
        }
    }
}

