using System;
using System.Collections.Generic;
using Common.Defs;
using Common.Core;
using Common.Entities.Tools;
using Unity.Entities;
using Unity.Jobs;

namespace Game.Core
{
    public interface ILogic : IComponentData, IDefineable<ILogicDef>
    {
        ILogicDef Def { get; }
        int CurrentJob { get; set; }
    }

    public class LogicDef: BaseLogicDef<Logic>
    {
        public static void Initialize()
        {
            m_System = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<LogicSystem>();
            m_System.Configure
                .TransitionEnter<InitPlaceJob>();
        }

        protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            base.AddComponentData(entity, manager, conversionSystem);
            manager.AddComponent<Logic.State>(entity);
        }

        public override int GetTransition(int value, JobResult jobResult)
        {
            IEnumerable<ILogicPart> list = Logic.GetEnterTransition();
            //if (value != 0)
            //    list = Logic.GetTransition(value, jobResult);
            var result = Random(list);
            return Logic.GetID(result);
        }
    }
    
    
    public struct Logic: ILogic
    {
        private ReferenceObject<ILogicDef> m_Def;
        public int currentJob;

        public struct State: IComponentData
        {
            public JobState Value;
            public static void SetState(ref EntityCommandBuffer.ParallelWriter writer, ref Entity entity, JobState state, int sortKey)
            {
                writer.SetComponent(sortKey, entity, new State { Value = state });
            }
        }

        #region ILogic
        public ILogicDef Def => m_Def.Link;
        public int CurrentJob { get => currentJob; set => currentJob = value; }
        #endregion

        public Logic(ReferenceObject<ILogicDef> def)
        {
            m_Def = def;
            currentJob = 0;
        }
    }
}