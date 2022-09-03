using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace Game.Model.Logics
{
    using Core;
    using World;
    using Skills;

    public unsafe struct FindTargetPlaceJob : ILogicJob
    {
        public float Weight => 1;
        [ReadOnly] private ComponentDataFromEntity<Translation> m_InputTranslation;
        [ReadOnly] private ComponentDataFromEntity<Teams> m_InputTeams;
        [ReadOnly] private ComponentDataFromEntity<Move.Moving> m_InputMove;

        public FindTargetPlaceJob(LogicSystem system)
        {
            m_InputTranslation = system.GetComponentDataFromEntity<Translation>(true);
            m_InputTeams = system.GetComponentDataFromEntity<Teams>(true);
            m_InputMove = system.GetComponentDataFromEntity<Move.Moving>(true);
        }

        public void Execute(ExecuteContext context)
        {
            var enemyTeams = m_InputTeams[context.Entity];
            var selfRealPosotion = m_InputTranslation[context.Entity];
            var moving = m_InputMove[context.Entity];
            
            var enemy = FindTarget.FindEnemy(enemyTeams.EnemyTeams, selfRealPosotion.Value, float.MaxValue, m_InputTranslation, m_InputTeams);
            
            if (enemy != Entity.Null)
            {
                var pos = m_InputMove[enemy].CurrentPosition;
                if (!Map.GeneratePosition(Map.Singleton, ref pos))
                {
                    context.Callback.Invoke(context.Entity, JobResult.Error);
                    return;
                }
                Move.MoveTo(context.Entity, pos, context.Callback);
            }
            else
            {
                context.Callback.Invoke(context.Entity, JobResult.Error);
            }
        }
    }
}

