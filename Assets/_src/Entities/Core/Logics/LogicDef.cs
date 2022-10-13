using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Common.Defs;

namespace Game.Model.Logics
{
    using Core;

    public interface ILogicDef : IDef
    {
        LogicStateMachine Logic { get; }
        int GetTransition(LogicStateMachine.StateJobs jobs, int value, JobResult jobResult);
    }

    public abstract class BaseLogicDef<T> : ClassDef<T>, ILogicDef
        where T : struct, ILogic
    {
        protected static LogicSystem m_System;
        public LogicStateMachine Logic => m_System.StateMachine;


        static System.Random m_Random = new System.Random();

        public abstract int GetTransition(LogicStateMachine.StateJobs jobs, int value, JobResult jobResult);

        /*
        protected static ILogicJob Random(IEnumerable<ILogicJob> list)
        {
            float max = 0;
            float weight = list.Sum((item) => item.Weight);
            ILogicJob result = null;
            foreach (ILogicJob iter in list)
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
        */
    }
}