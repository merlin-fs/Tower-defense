using System;

namespace Game.Core
{
    public unsafe interface ILogicPart
    {
        void Init(LogicSystem system);
        void Execute(ExecuteContext context);
        float Weight { get; }
    }
} 