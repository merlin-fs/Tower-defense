using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;

[assembly: RegisterGenericJobType(typeof(Game.Model.Logics.LogicSystem<Game.Model.Logics.EnemyLogic, Game.Model.Logics.EnemyLogic.State>.LogicJob))]

namespace Game.Model.Logics
{
    using Core;


    [Defineable(typeof(EnemyLogic))]
    public class EnemyLogicDef : BaseLogicDef<EnemyLogic>
    {
        public partial class LogicSystem : LogicSystem<EnemyLogic, EnemyLogic.State> { }

        public static void Initialize()
        {
            m_System = Unity.Entities.World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<LogicSystem>();
            
            m_System.Configure
                .TransitionEnter<EnemySquadDef.PlaceUnitsJob>()
                .Transition<EnemySquadDef.PlaceUnitsJob, FindTargetPlaceJob>()
                .Transition<FindTargetPlaceJob, FindTargetPlaceJob>(JobResult.Error);
        }

        protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            base.AddComponentData(entity, manager, conversionSystem);
            manager.AddComponent<EnemyLogic.State>(entity);
            manager.AddComponent<EnemyLogic.Target>(entity);
        }

        protected override void AddComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            base.AddComponentData(entity, writer, sortKey);
            writer.AddComponent<EnemyLogic.State>(sortKey, entity);
            writer.AddComponent<EnemyLogic.Target>(sortKey, entity);
        }

        public override int GetTransition(int value, JobResult jobResult)
        {
            IEnumerable<ILogicPart> list = Logic.GetEnterTransition();
            if (value != 0)
                list = Logic.GetTransition(value, jobResult);
            var result = Random(list);
            return Logic.GetID(result);
        }
    }
}