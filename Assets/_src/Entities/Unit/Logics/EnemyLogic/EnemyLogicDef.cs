using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;

[assembly: RegisterGenericJobType(typeof(Game.Core.LogicSystem<Game.Model.Units.Logics.EnemyLogic, Game.Model.Units.Logics.EnemyLogic.State>.LogicJob))]

namespace Game.Model.Units.Logics
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
                .TransitionEnter<InitPlaceJob>()
                .Transition<InitPlaceJob, FindTargetPlaceJob>();
        }

        protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            base.AddComponentData(entity, manager, conversionSystem);
            manager.AddComponent<EnemyLogic.State>(entity);
            manager.AddComponent<EnemyLogic.Target>(entity);
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