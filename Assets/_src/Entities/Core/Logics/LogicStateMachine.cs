using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Jobs;

namespace Game.Model.Logics
{
    using Common.Core;
    using Core;

    public partial class LogicStateMachine
    {
        /*
        private Dictionary<int, JobInfo> m_Jobs = new Dictionary<int, JobInfo>();
        private List<JobInfo> m_Enter = new List<JobInfo>();
        */
       
        public struct StateJobs
        {
            private ReferenceObject<LogicStateMachine> m_Owner;
            private LogicStateMachine Owner => m_Owner.Link;

            /*
            private ReferenceObject<Dictionary<int, ILogicJob>> m_Jobs;
            private Dictionary<int, ILogicJob> Jobs => m_Jobs.Link;
            */

            internal StateJobs(LogicStateMachine owner, LogicSystem system)
            {
                m_Owner = new ReferenceObject<LogicStateMachine>(owner);
                /*
                m_Jobs = new ReferenceObject<Dictionary<int, ILogicJob>>(new Dictionary<int, ILogicJob>());

                foreach(var iter in Owner.m_Jobs.Values)
                {
                    Jobs.Add(iter.ID, iter.Create(system));
                }
                */
            }

            /*
            internal ILogicJob GetJob(int id) => Jobs[id];

            public void Dispose(JobHandle inputDeps)
            {
                //m_Owner;
                //m_Jobs;
            }

            public IEnumerable<ILogicJob> GetEnterTransition()
            {
                var jobs = Jobs;
                return Owner.m_Enter
                    .Select(i => jobs[i.ID]);
            }

            public IEnumerable<ILogicJob> GetTransition(int current, JobResult jobResult)
            {
                return Owner.m_Jobs.TryGetValue(current, out JobInfo info)
                    ? info.TryGetTransition(this, jobResult)
                    : null;
            }

            public bool TryGetJob(int id, out ILogicJob value)
            {
                var result = Owner.m_Jobs.TryGetValue(id, out JobInfo info);
                value = result
                    ? Jobs[info.ID]
                    : null;
                return result;
            }

            public bool TryGetJob<T>(int id, out T value)
                where T : struct, ILogicJob
            {
                var result = Owner.m_Jobs.TryGetValue(id, out JobInfo info);
                value = result
                    ? (T)Jobs[info.ID]
                    : default;
                return result;
            }
            */
        }

        /*
        internal LogicStateMachine()
        {
        }

        public StateJobs PrepareJobs(LogicSystem system)
        {
            return new StateJobs(this, system);
        }


        private void AddTransition<From, To>(JobResult jobResult)
            where From : struct, ILogicJob
            where To : struct, ILogicJob
        {
            var from = GetInfo(typeof(From), true);
            var to = GetInfo(typeof(To), true);
            from.AddTransition(to, jobResult);
        }

        private void Add<State>()
            where State : struct, ILogicJob
        {
            GetInfo(typeof(State), true);
        }

        private void AddEnterTransition<To>()
            where To : struct, ILogicJob
        {
            var to = GetInfo(typeof(To), true);
            m_Enter.Add(to);
        }

        public int GetID(ILogicJob value)
        {
            return value == null 
                ? 0 
                : value.GetType().GetHashCode();
        }

        private JobInfo GetInfo(Type type, bool need)
        {
            var id = type.GetHashCode();
            if (!m_Jobs.TryGetValue(id, out JobInfo value) && need)
            {
                value = new JobInfo(type, id);
                m_Jobs.Add(id, value);
            }
            return value;
        }

        private class JobInfo
        {
            public int ID { get; }
            private Type m_Type;
            private List<JobInfo> m_Done = new List<JobInfo>();
            private List<JobInfo> m_Error = new List<JobInfo>();

            public JobInfo(Type type, int id)
            {
                ID = id;
                m_Type = type;
            }

            public ILogicJob Create(LogicSystem system)
            {
                return (ILogicJob)Activator.CreateInstance(m_Type, system);
            }

            private ILogicJob Job(StateJobs states) => states.GetJob(ID);

            public void AddTransition(JobInfo info, JobResult jobResult)
            {
                switch (jobResult)
                {
                    case JobResult.Done: m_Done.Add(info); break;
                    case JobResult.Error: m_Error.Add(info); break;
                }
            }

            public IEnumerable<ILogicJob> TryGetTransition(StateJobs states, JobResult jobResult)
            {
                switch (jobResult)
                {
                    case JobResult.Done: return m_Done.Select(info => info.Job(states));
                    case JobResult.Error: return m_Error.Select(info => info.Job(states));
                }
                throw new NotImplementedException();
            }
        }
        */
    }

    /*
    stateMachine.Configure
        .Transition<default, InitPlace>()

        .Transition<InitPlace, Wait>()

        .Transition<Wait, FindTarget>()
        .Transition<FindTarget, BuildPath>()
        .Transition<FindTarget, Wait>(JobResult.Error);

        .Transition<BuildPath, MoveToPoint>()



    */
}