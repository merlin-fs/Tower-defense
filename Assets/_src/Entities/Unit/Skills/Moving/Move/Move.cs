using System;
using Common.Core;
using Common.Defs;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Burst;

namespace Game.Model.Units.Skills
{
    using Core;
    public partial class Move
    {
        public struct Commands: IComponentData
        {
            public State Value;
            public int2 TargetPosition;
            public FunctionPointer<StateCallback> Callback;
        }

        public enum State
        {
            None,
            Init,
            FindPath,
            FindPathDone,
            MoveToPoint,
            Error,
            Done,
        }

        public struct Moving : ISkill, ICallbackComponent, IDefineable<MovingDef>
        {
            public ReferenceObject<MovingDef> Def;

            public int2 TargetPosition;
            public int2 CurrentPosition;

            public float PathPrecent;
            public Entity JobResult;
            public Entity StateEntity;

            public Moving(ReferenceObject<MovingDef> def)
            {
                Def = def;
                TargetPosition = default;
                CurrentPosition = default;
                PathPrecent = default;
                JobResult = Entity.Null;
                StateEntity = Entity.Null;
            }
        }
    }
}