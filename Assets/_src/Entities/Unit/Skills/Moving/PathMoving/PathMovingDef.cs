using System;

namespace Game.Model.Units
{
    using Defs;

    [Defineable(typeof(PathMoving))]
    public class PathMovingDef : BaseSkillDef
    {
        public float Speed = 80;
    }
}