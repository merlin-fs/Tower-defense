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
            public SquadDef Def => m_Def.Link;
            private ReferenceObject<SquadDef> m_Def;
            
            public Entity Leader;

            public Data(ReferenceObject<SquadDef> def)
            {
                m_Def = def;
                Leader = Entity.Null;
            }
        }

        public struct UnitLink: IBufferElementData
        {
            public Entity Unit;
        }

        public struct UnitPosition : IBufferElementData
        {
            public int2 Position;
        }

        public struct Unit : IComponentData
        {
            public Entity Squad;
            public int Index;
        }
    }
}
