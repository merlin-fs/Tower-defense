using System;
using Common.Defs;
using Unity.Entities;

namespace Game.Core
{
    public enum JobResult
    {
        Error,
        Done,
    }

    public enum JobState
    {
        None,
        Running,
        Error,
    }

    public delegate void StateCallback(EntityCommandBuffer.ParallelWriter writer, Entity entity, JobResult state, int sortKey);

    public interface ICallbackComponent
    {
    }
}
