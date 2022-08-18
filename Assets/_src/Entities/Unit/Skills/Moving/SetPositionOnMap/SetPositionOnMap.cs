using System;

using Common.Core;
using Common.Defs;
using Unity.Mathematics;

namespace Game.Model.Units
{
    public struct SetPositionOnMap : ISkill, IDefineable<SetPositionOnMapDef>
    {
        public ReferenceObject<SetPositionOnMapDef> Def;
        public SetPositionOnMap(ReferenceObject<SetPositionOnMapDef> def)
        {
            Def = def;
            InitPosition = Def.Link.InitPosition;
            TargetPosition = Def.Link.TargetPosition;
            PathPrecent = 0;
            PathLength = 0;
            PathDeltaTime = 0;
        }

        public int2 InitPosition;
        public int2 TargetPosition;
        
        public float PathPrecent;
        public float PathLength;
        public float PathDeltaTime;
    }
}