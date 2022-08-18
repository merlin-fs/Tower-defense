using System;

using Common.Core;
using Common.Defs;

namespace Game.Model.Units
{
    public struct Cannon : ISkill, IDefineable<CannonDef>
    {
        public ReferenceObject<CannonDef> Def;
        public Cannon(ReferenceObject<CannonDef> def)
        {
            Def = def;
            TimeShot = 0;
            TimeFind = 0;
        }

        public float TimeShot;
        public float TimeFind;
    }
}