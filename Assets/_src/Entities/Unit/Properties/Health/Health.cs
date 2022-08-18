using System;

using Common.Core;
using Common.Defs;

namespace Game.Model.Units
{
    using Defs;

    public struct Health : IProperty, IDefineable<IPropertyDef>
    {
        public ReferenceObject<IPropertyDef> Def;
        public Health(ReferenceObject<IPropertyDef> def)
        {
            Def = def;
            Value = 0;
        }

        public float Value;
        public IProperty Property => this;
        float IProperty.Value => Value;
        float IProperty.Normalize => Value / Def.Link.Value;
    }
}