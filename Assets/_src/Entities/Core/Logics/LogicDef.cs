using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Common.Defs;

namespace Game.Core
{
    public interface ILogicDef : IDef
    {
        LogicStateMachine Logic { get; }
        int GetTransition(int value, JobResult jobResult);
    }

    public abstract class BaseLogicDef<T> : ClassDef<T>, ILogicDef
        where T : struct, ILogic
    {
        protected static LogicSystem m_System;
        public LogicStateMachine Logic => m_System.StateMachine;


        static System.Random m_Random = new System.Random();

        public abstract int GetTransition(int value, JobResult jobResult);

        protected static ILogicPart Random(IEnumerable<ILogicPart> list)
        {
            float max = 0;
            float weight = list.Sum((item) => item.Weight);
            ILogicPart result = null;
            foreach (ILogicPart iter in list)
            {
                float rnd = (float)m_Random.NextDouble();
                float rand = Mathf.Pow(rnd, 1 / (iter.Weight / weight));
                if (rand > max)
                {
                    max = rand;
                    result = iter;
                }
            }
            return result;
        }
    }
}