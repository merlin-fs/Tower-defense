using System;
using Unity.Entities;
using Common.Core;
using Common.Defs;

namespace Game.Model.Parts
{
    public struct Turret : IPart, IDefineable<TurretDef>
    {
        public ReferenceObject<TurretDef> Def;
        public Turret(ReferenceObject<TurretDef> def)
        {
            Def = def;
            WaitTimer = 0;
            WaitRndTimer = 0;
            CurrentRotationSpeed = Def.Link.IdleRotationSpeed;
            RotationCorrectionTime = 0;
            Time = 0.5f;
            Direct = true;
            RndTime = 1f;
            Entity = Entity.Null;
        }

        public Entity Entity;
        public float WaitTimer;
        public float WaitRndTimer;
        public float CurrentRotationSpeed;
        public float RotationCorrectionTime;
        public float Time;
        public float RndTime;
        public bool Direct;
    }
}
