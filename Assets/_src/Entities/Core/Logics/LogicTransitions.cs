using System;

namespace Game.Model.Logics
{
    using Core;

    public abstract partial class LogicSystem
    {
        public LogicStateMachine.Configuration Configure => new LogicStateMachine.Configuration(m_StateMachine);
        private LogicStateMachine m_StateMachine;
        public LogicStateMachine StateMachine => m_StateMachine;
    }
    
    public partial class LogicStateMachine
    {
        public class Configuration
        {
            protected LogicStateMachine m_Owner;

            internal Configuration(LogicStateMachine owner)
            {
                m_Owner = owner;
            }

            /*
            public Configuration Add<State>()
                where State : struct, ILogicJob
            {
                m_Owner.Add<State>();
                return this;
            }

            public Configuration Transition<From, To>(JobResult jobResult = JobResult.Done)
                where From : struct, ILogicJob
                where To : struct, ILogicJob
            {
                m_Owner.AddTransition<From, To>(jobResult);
                return this;
            }

            public Configuration TransitionEnter<To>()
                where To : struct, ILogicJob
            {
                m_Owner.AddEnterTransition<To>();
                return this;
            }
            */
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