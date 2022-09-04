using System;
using Unity.Entities;
using Unity.Collections;

namespace Game.Model.Logics
{
    using Core;
    using Skills;
    using Unity.Burst;
    using World;

    public struct MovingJob : ILogicJob
    {
        public float Weight => 1;

        public MovingJob(LogicSystem system) { }

        public void Execute(ExecuteContext context, FunctionPointer<StateCallback> callback)
        {
            Move.MoveTo(context.Entity, callback);
        }
    }
}

