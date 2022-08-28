using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Common.Defs;
using Common.Core;

namespace Game.Model.Units
{
    using Model.Core;
    using Model.Skills;
    using Model.Properties;
    using Model.Damages;
    using Model.Parts;
    using Model.Logics;


    public interface IUnitDef: IDef
    {
        IReadOnlyCollection<ISkillDef> Skills { get; }
        IReadOnlyCollection<IPropertyDef> Propirties { get; }
        IReadOnlyCollection<IPartDef> Parts { get; }
        ILogicDef Logic { get; }
        Entity EntityPrefab { get; }
    }

    [CreateAssetMenu(fileName = "Unit", menuName = "Defs/Unit")]
    [Defineable(typeof(Unit))]
    public class UnitDef : ScriptableDef<IUnit>, IUnitDef, IIdentifiable<ObjectTypeID>
    {
        ObjectTypeID IIdentifiable<ObjectTypeID>.ID => GetNameID;
        [SerializeField]
        GameObject m_Prefab;

        [SerializeReference, Reference()]
        ISkillDef[] m_Skills;

        [SerializeReference, Reference()]
        IPropertyDef[] m_Propirties;

        [SerializeReference, Reference()]
        IPartDef[] m_Parts;

        [SerializeReference, Reference()]
        ILogicDef m_Logic;

        private Entity m_EntityPrefab;


        IReadOnlyCollection<ISkillDef> IUnitDef.Skills => m_Skills;
        IReadOnlyCollection<IPropertyDef> IUnitDef.Propirties => m_Propirties;
        IReadOnlyCollection<IPartDef> IUnitDef.Parts => m_Parts;
        ILogicDef IUnitDef.Logic => m_Logic;
        Entity IUnitDef.EntityPrefab => m_EntityPrefab;

        public virtual void Init(Entity entityPrefab)
        {
            m_EntityPrefab = entityPrefab;
        }

        protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            base.AddComponentData(entity, manager, conversionSystem);
            manager.AddComponentData<WeaponReady>(entity, true);


            AddComponents(m_Skills);
            AddComponents(m_Propirties);
            AddComponents(m_Parts);
            m_Logic.AddComponentData(entity, manager, conversionSystem);

            void AddComponents(IDef[] items)
            {
                if (items != null)
                    foreach (var iter in items)
                    {
                        iter.AddComponentData(entity, manager, conversionSystem);
                    }
            }
        }

        public GameObject GetPrefab()
        {
            return m_Prefab;
        }
    }
}