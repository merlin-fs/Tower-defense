using System;
using Unity.Entities;
using Unity.Mathematics;
using Common.Defs;
using Common.Core;

namespace Game.Model
{
    public partial class Squad
    {
        public struct Data : IComponentData, IDefineable<SquadDef>
        {
            public ReferenceObject<SquadDef> Def;
            
            public Entity Leader;

            public Data(ReferenceObject<SquadDef> def)
            {
                Def = def;
                Leader = Entity.Null;
            }
        }
    }
}
