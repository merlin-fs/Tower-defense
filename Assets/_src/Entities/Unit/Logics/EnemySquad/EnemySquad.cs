using System;
using Unity.Entities;
using Unity.Mathematics;
using Common.Core;


namespace Game.Model.Logics
{
    using Core;
    using World;
    public struct EnemySquad : ILogic
    {
        public struct Target: IComponentData
        {
            public Entity Entity;
        }

        private ReferenceObject<ILogicDef> m_Def;
        public int currentJob;

        public struct State : ILogicState
        {
            public JobState value;
            public JobState Value { get => value; set => this.value = value; }
            public void SetState(EntityCommandBuffer.ParallelWriter writer, Entity entity, JobState state, int sortKey)
            {
                writer.SetComponent(sortKey, entity, new State { Value = state });
            }
        }

        #region ILogic
        public ILogicDef Def => m_Def.Link;
        public EnemySquadDef SquadDef => (EnemySquadDef)m_Def.Link;
        public int CurrentJob { get => currentJob; set => currentJob = value; }
        #endregion

        public EnemySquad(ReferenceObject<ILogicDef> def)
        {
            m_Def = def;
            currentJob = 0;
        }
    }
}