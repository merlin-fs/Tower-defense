using System;
using Unity.Entities;

namespace Game.Model.Core
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

    public delegate void StateCallback(Entity entity, JobResult state);
}
