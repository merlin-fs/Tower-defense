using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;

[assembly: RegisterGenericJobType(typeof(Game.Model.Logics.LogicSystem<Game.Model.Logics.EnemyLogic, Game.Model.Logics.EnemyLogic.State>.LogicJob))]

namespace Game.Model.Logics
{
    using Core;

    [Defineable(typeof(EnemyLogic))]
    public partial class EnemyLogicDef : BaseLogicDef<EnemyLogic>
    {
        public partial class LogicSystem : LogicSystem<EnemyLogic, EnemyLogic.State> { }

        public static void Initialize()
        {
            m_System = Unity.Entities.World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<LogicSystem>();

            m_System.Configure
                .Add<InitPlaceJob>()

                .TransitionEnter<EnemySquadLogicDef.PlaceUnitsJob>()
                .Transition<EnemySquadLogicDef.PlaceUnitsJob, EnemySquadLogicDef.WaitMoveSquadJob>()

                .Transition<EnemySquadLogicDef.WaitMoveSquadJob, EnemySquadLogicDef.WaitMoveSquadJob>(JobResult.Error)
                .Transition<EnemySquadLogicDef.WaitMoveSquadJob, EnemySquadLogicDef.FindPathUnitsJob>()
                
                .Transition<EnemySquadLogicDef.FindPathUnitsJob, MovingJob>()
                .Transition<MovingJob, EnemySquadLogicDef.FindPathUnitsJob>();

                //.Transition<EnemySquadLogicDef.PlaceUnitsJob, FindTargetPlaceJob>()
                //.Transition<FindTargetPlaceJob, FindTargetPlaceJob>(JobResult.Error);
        }

        protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            base.AddComponentData(entity, manager, conversionSystem);
            manager.AddComponent<EnemyLogic.State>(entity);
            manager.AddComponent<EnemyLogic.Target>(entity);
            manager.AddComponent<EnemyLogic.WorkTime>(entity);
        }

        protected override void AddComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            base.AddComponentData(entity, writer, sortKey);
            writer.AddComponent<EnemyLogic.State>(sortKey, entity);
            writer.AddComponent<EnemyLogic.Target>(sortKey, entity);
            writer.AddComponent<EnemyLogic.WorkTime>(sortKey, entity);
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