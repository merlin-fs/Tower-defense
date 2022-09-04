using System;

using Game.Model.Core;
using Unity.Burst;

namespace Game.Model.Logics
{
    public interface ILogicJob
    {
        //public ILogicJob(LogicSystem system) { }
        void Execute(ExecuteContext context, FunctionPointer<StateCallback> callback);
        float Weight { get; }
    }
} 