using System;

using Common.Core;
using Common.Defs;

namespace Game.Model.Units
{
    public struct Unit : IUnit, IDefineable<UnitDef>
    {
        public ReferenceObject<UnitDef> Def;
        public Unit(ReferenceObject<UnitDef> def)
        {
            Def = def;
        }
    }
}