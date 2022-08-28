using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace Game.Model.Logics
{
    using Core;

    public partial class LogicStateMachine
    {
        private Dictionary<int, JobInfo> m_Jobs = new Dictionary<int, JobInfo>();
        private List<JobInfo> m_Enter = new List<JobInfo>();
        private EntityManager m_EntityManager;
        public SystemBase System { get; }
        public EntityCommandBufferSystem CommandBuffer { get; }

        public IEnumerable<ILogicPart> Parts => m_Jobs.Values.Select(i => i.Job);

        internal LogicStateMachine(EntityManager entityManager, SystemBase system, EntityCommandBufferSystem commandBuffer)
        {
            m_EntityManager = entityManager;
            CommandBuffer = commandBuffer;
            System = system;
        }

        private void AddTransition<From, To>(JobResult jobResult)
            where From : class, ILogicPart
            where To : class, ILogicPart
        {
            var from = GetInfo(typeof(From), true);
            var to = GetInfo(typeof(To), true);
            from.AddTransition(to, jobResult);
        }

        private void AddEnterTransition<To>()
            where To : class, ILogicPart
        {
            var to = GetInfo(typeof(To), true);
            m_Enter.Add(to);
        }

        public IEnumerable<ILogicPart> GetEnterTransition()
        {
            return m_Enter.Select(i => i.Job);
        }

        public IEnumerable<ILogicPart> GetTransition(int current, JobResult jobResult)
        {
            return m_Jobs.TryGetValue(current, out JobInfo info) 
                ? info.TryGetTransition(jobResult) 
                : null;
        }

        public int GetID(ILogicPart value)
        {
            return value == null 
                ? 0 
                : value.GetType().GetHashCode();
        }

        public bool TryGetJob(int id, out ILogicPart value)
        {
            var result = m_Jobs.TryGetValue(id, out JobInfo info);
            value = result 
                ? info.Job 
                : null;
            return result;
        }

        private JobInfo GetInfo(Type type, bool need)
        {
            var id = type.GetHashCode();
            if (!m_Jobs.TryGetValue(id, out JobInfo value) && need)
            {
                value = new JobInfo(type, id, m_EntityManager);
                m_Jobs.Add(id, value);
            }
            return value;
        }

        private class JobInfo
        {
            public ILogicPart Job { get; }
            public int ID { get; }
            private List<JobInfo> m_Done = new List<JobInfo>();
            private List<JobInfo> m_Error = new List<JobInfo>();

            public JobInfo(Type type, int id, EntityManager manager)
            {
                Job = (ILogicPart)Activator.CreateInstance(type);//, new object[] { manager }
                ID = id;
            }
            public void AddTransition(JobInfo info, JobResult jobResult)
            {
                switch (jobResult)
                {
                    case JobResult.Done: m_Done.Add(info); break;
                    case JobResult.Error: m_Error.Add(info); break;
                }
            }

            public IEnumerable<ILogicPart> TryGetTransition(JobResult jobResult)
            {
                switch (jobResult)
                {
                    case JobResult.Done: return m_Done.Select(info => info.Job);
                    case JobResult.Error: return m_Error.Select(info => info.Job);
                }
                throw new NotImplementedException();
            }
        }
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