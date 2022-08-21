using System;
using System.Collections.Generic;
using Common.Defs;
using Common.Core;
using Common.Entities.Tools;
using Unity.Entities;
using Unity.Jobs;

namespace Game.Core
{
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