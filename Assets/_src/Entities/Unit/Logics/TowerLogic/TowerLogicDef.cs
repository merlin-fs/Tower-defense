using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;

[assembly: RegisterGenericJobType(typeof(Game.Model.Logics.LogicSystem<Game.Model.Logics.TowerLogic, Game.Model.Logics.TowerLogic.State>.LogicJob))]

namespace Game.Model.Logics
{
    using Core;

    [Defineable(typeof(TowerLogic))]
    public class TowerLogicDef : BaseLogicDef<TowerLogic>
    {
        public partial class LogicSystem : LogicSystem<TowerLogic, TowerLogic.State> {}
        public static void Initialize()
        {
            m_System = Unity.Entities.World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<LogicSystem>();
            m_System.Configure
                .TransitionEnter<InitPlaceJob>();
        }

        protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            base.AddComponentData(entity, manager, conversionSystem);
            manager.AddComponent<TowerLogic.State>(entity);
        }

        protected override void AddComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            base.AddComponentData(entity, writer, sortKey);
            writer.AddComponent<TowerLogic.State>(sortKey, entity);
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