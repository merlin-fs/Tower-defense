using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;

[assembly: RegisterGenericJobType(typeof(Game.Model.Logics.LogicSystem<Game.Model.Logics.EnemySquadLogic, Game.Model.Logics.EnemySquadLogic.State>.LogicJob))]

namespace Game.Model.Logics
{
    using Core;


    [Defineable(typeof(EnemySquadLogic))]
    public partial class EnemySquadLogicDef : BaseLogicDef<EnemySquadLogic>
    {
        public partial class System : LogicSystem<EnemySquadLogic, EnemySquadLogic.State> { }

        public static void Initialize()
        {
            m_System = Unity.Entities.World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<System>();
            m_System.Configure
                .TransitionEnter<InitSquadJob>()
                .Transition<InitSquadJob, FindPathToTargetJob>()
                .Transition<FindPathToTargetJob, MovingJob>();
        }

        protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            base.AddComponentData(entity, manager, conversionSystem);
            manager.AddComponent<EnemySquadLogic.State>(entity);
            manager.AddComponent<EnemySquadLogic.Target>(entity);
        }

        protected override void AddComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            base.AddComponentData(entity, writer, sortKey);
            writer.AddComponent<EnemySquadLogic.State>(sortKey, entity);
            writer.AddComponent<EnemySquadLogic.Target>(sortKey, entity);
        }

        public override int GetTransition(LogicStateMachine.StateJobs jobs, int value, JobResult jobResult)
        {
            IEnumerable<ILogicJob> list = jobs.GetEnterTransition();
            if (value != 0)
                list = jobs.GetTransition(value, jobResult);
            var result = Random(list);
            return Logic.GetID(result);
        }
    }
}