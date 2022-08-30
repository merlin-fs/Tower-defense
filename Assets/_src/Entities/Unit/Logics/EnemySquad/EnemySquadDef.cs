using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;

[assembly: RegisterGenericJobType(typeof(Game.Model.Logics.LogicSystem<Game.Model.Logics.EnemySquad, Game.Model.Logics.EnemySquad.State>.LogicJob))]

namespace Game.Model.Logics
{
    using Core;


    [Defineable(typeof(EnemySquad))]
    public partial class EnemySquadDef : BaseLogicDef<EnemySquad>
    {
        public partial class System : LogicSystem<EnemySquad, EnemySquad.State> { }

        public static void Initialize()
        {
            m_System = Unity.Entities.World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<System>();
            m_System.Configure
                .TransitionEnter<InitSquadJob>();
        }

        protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            base.AddComponentData(entity, manager, conversionSystem);
            manager.AddComponent<EnemySquad.State>(entity);
            manager.AddComponent<EnemySquad.Target>(entity);
        }

        protected override void AddComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            base.AddComponentData(entity, writer, sortKey);
            writer.AddComponent<EnemySquad.State>(sortKey, entity);
            writer.AddComponent<EnemySquad.Target>(sortKey, entity);
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