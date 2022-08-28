using System;

namespace Game.Model.Logics
{
    public unsafe interface ILogicPart
    {
        void Init(LogicSystem system);
        void Execute(ExecuteContext context);
        float Weight { get; }
    }
} 