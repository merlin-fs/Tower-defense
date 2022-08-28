using System;

using Common.Core;
using Common.Defs;

namespace Game.Model.Damages
{
    public struct DamageSimple : IDamage, IDefineable<IDamageDef>
    {
        public ReferenceObject<IDamageDef> Def;
        public DamageSimple(ReferenceObject<IDamageDef> def)
        {
            Def = def;
        }
    }
}