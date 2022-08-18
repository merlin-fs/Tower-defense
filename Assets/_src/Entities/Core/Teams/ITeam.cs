using System;

using Common.Core;
using Common.Defs;
using Unity.Entities;

namespace Game.Model.Units
{
    using Defs;

    public struct Teams: IDefineable<TeamDef>, IComponentData
    {
        public ReferenceObject<TeamDef> Def;

        public Teams(ReferenceObject<TeamDef> def)
        {
            Def = def;
            Team = 0;
            EnemyTeams = 0;
        }

        public uint Team;
        public uint EnemyTeams;
    }
}