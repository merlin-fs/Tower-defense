using System;
using Unity.Entities;
using Common.Defs;

namespace Game.Model.Units.Defs
{
    public interface ISkillDef: IDef
    {
    }

    public abstract class BaseSkillDef : ClassDef<ISkill>, ISkillDef
    {
        protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            base.AddComponentData(entity, manager, conversionSystem);
        }
    }
}