using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;

[assembly: RegisterGenericJobType(typeof(Game.Model.Logics.LogicSystem<Game.Model.Logics.EnemySquad, Game.Model.Logics.EnemySquad.State>.LogicJob))]

namespace Game.Model.Logics
{
    using Core;


    [Defineable(typeof(EnemySquad))]
    public class EnemySquadDef : BaseLogicDef<EnemySquad>
    {
        public partial class LogicSystem : LogicSystem<EnemySquad, EnemySquad.State> { }

        public static void Initialize()
        {
            m_System = Unity.Entities.World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<LogicSystem>();
            m_System.Configure
                .TransitionEnter<InitPlaceJob>()
                .Transition<InitPlaceJob, FindTargetPlaceJob>()
                .Transition<FindTargetPlaceJob, FindTargetPlaceJob>(JobResult.Error);
        }

        protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            base.AddComponentData(entity, manager, conversionSystem);
            manager.AddComponent<EnemySquad.State>(entity);
            manager.AddComponent<EnemySquad.Target>(entity);
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