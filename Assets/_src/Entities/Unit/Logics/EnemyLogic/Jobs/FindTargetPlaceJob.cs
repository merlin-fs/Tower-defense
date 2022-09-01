using System;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace Game.Model.Logics
{
    using Core;
    using World;
    using Skills;

    public unsafe class FindTargetPlaceJob : ILogicPart
    {
        public float Weight => 1;
        [ReadOnly] public ComponentDataFromEntity<Translation> InputTranslation;
        [ReadOnly] public ComponentDataFromEntity<Teams> InputTeams;
        [ReadOnly] public ComponentDataFromEntity<Move.Moving> InputMove;

        public void Init(LogicSystem system)
        {
            InputTranslation = system.GetComponentDataFromEntity<Translation>(true);
            InputTeams = system.GetComponentDataFromEntity<Teams>(true);
            InputMove = system.GetComponentDataFromEntity<Move.Moving>(true);
        }

        public void Execute(ExecuteContext context)
        {
            var enemyTeams = InputTeams[context.Entity];
            var selfRealPosotion = InputTranslation[context.Entity];
            var moving = InputMove[context.Entity];
            
            var enemy = FindTarget.FindEnemy(enemyTeams.EnemyTeams, selfRealPosotion.Value, float.MaxValue, InputTranslation, InputTeams);
            
            if (enemy != Entity.Null)
            {
                var pos = InputMove[enemy].CurrentPosition;
                if (!Map.GeneratePosition(Map.Singleton, ref pos))
                {
                    context.Callback.Invoke(context.Writer, context.Entity, JobResult.Error, context.SortKey);
                    return;
                }
                Move.MoveTo(context.Entity, pos, context.Callback, context.SortKey);
            }
            else
            {
                context.Callback.Invoke(context.Writer, context.Entity, JobResult.Error, context.SortKey);
            }
        }
    }
}

