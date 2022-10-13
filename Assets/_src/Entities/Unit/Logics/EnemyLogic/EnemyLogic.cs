using System;
using Unity.Entities;
using Unity.Mathematics;
using Common.Core;

/*
namespace Game.Model.Logics
{
    using Core;
    using World;

    public struct EnemyLogic : ILogic
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
        }

        public struct WorkTime : IComponentData
        {
            public float Time;
        }

        #region ILogic
        public ILogicDef Def => m_Def.Link;
        public int CurrentJob { get => currentJob; set => currentJob = value; }
        public IComponentData GetNextTransition(int current)
        {
            return null;
        }
        #endregion

        public EnemyLogic(ReferenceObject<ILogicDef> def)
        {
            m_Def = def;
            currentJob = 0;
        }
    }
}
*/