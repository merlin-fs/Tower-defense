using System;

namespace Game.Model.Logics
{
    public interface ILogicJob
    {
        //public ILogicJob(LogicSystem system) { }
        void Execute(ExecuteContext context);
        float Weight { get; }
    }
} 