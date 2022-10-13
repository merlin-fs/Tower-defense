using System;
using Common.Defs;
using Unity.Entities;

namespace Game.Model.Logics
{
    using Core;

    public interface ILogicState : IComponentData
    {
        JobState Value { get; set; }
    }

    public interface ILogic : ILogicState, IDefineable<ILogicDef>
    {
        int CurrentState { get; set; }
        ILogicState GetNextTransition(int current);
    }

    public interface ILogicPart : IJobEntityBatch
    {
        void Init(SystemBase system);
    }
}