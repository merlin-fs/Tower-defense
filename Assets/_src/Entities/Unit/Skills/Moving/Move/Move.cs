using System;
using Common.Core;
using Common.Defs;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Burst;

namespace Game.Model.Skills
{
    using Core;

    using Unity.Collections;

    public partial class Move
    {
        public struct Commands: IComponentData
        {
            public Command Value;
            public int2 TargetPosition;
            public FunctionPointer<StateCallback> Callback;
            
            public FixedList4096Bytes<int2> Path;
        }

        public enum Command
        {
            None,
            Init,
            FindPath,
            FindPathDone,
            MoveToPoint,
        }

        public struct Moving : ISkill, IDefineable<MovingDef>
        {
            public ReferenceObject<MovingDef> Def;

            public int2 TargetPosition;
            public int2 CurrentPosition;
            public float PathPrecent;
            public InternalState State;

            public enum InternalState
            {
                None,
                Init,
                FindPath,
                FindPathDone,
                MoveToPoint,
            }

            public Moving(ReferenceObject<MovingDef> def)
            {
                Def = def;
                TargetPosition = default;
                CurrentPosition = default;
                PathPrecent = default;
                State = default;
        }
        }
    }
}