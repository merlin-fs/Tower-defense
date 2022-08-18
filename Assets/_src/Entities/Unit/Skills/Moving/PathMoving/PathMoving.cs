using System;

using Common.Core;
using Common.Defs;

namespace Game.Model.Units
{
    public struct PathMoving : ISkill, IDefineable<PathMovingDef>
    {
        public ReferenceObject<PathMovingDef> Def;
        public PathMoving(ReferenceObject<PathMovingDef> def)
        {
            Def = def;
            ProgressDistance = 0;
        }

        public float ProgressDistance;
    }
}