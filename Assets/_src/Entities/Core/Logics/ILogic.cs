using System;
using Common.Defs;
using Unity.Entities;

namespace Game.Model.Logics
{
    using Core;

    public interface ILogic : IComponentData, IDefineable<ILogicDef>
    {
        ILogicDef Def { get; }
        int CurrentJob { get; set; }
    }

    public interface ILogicState : IComponentData
    {
        JobState Value { get; set; }

        void SetState(EntityCommandBuffer.ParallelWriter writer, Entity entity, JobState state, int sortKey);
    }
}