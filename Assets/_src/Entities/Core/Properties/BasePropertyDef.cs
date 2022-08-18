using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Common.Defs;

namespace Game.Model.Units.Defs
{
    public interface IPropertyDef : IDef
    {
        float Value { get; }

        /// <summary>
        /// ���������� �����.
        /// ������ ���� ����� �����, ������� ����� ���������.
        /// ���������, ����� ���������� ����������, ���������� �����.
        /// </summary>
        IReadOnlyCollection<IDamageDef> Absorb { get; }

        /// <summary>
        /// ������������� �����.
        /// ������ ���� ����� �����, � ������� ���� ��������������.
        /// ���������, ����� ���������� �������������, ������ ����.
        /// </summary>
        IReadOnlyCollection<IDamageDef> Resist { get; }
    }

    public abstract class BasePropertyDef<T> : ClassDef<T>, IPropertyDef
        where T : struct, IDefineable, IComponentData
    {
        [SerializeReference, Reference()]
        private IDamageDef[] m_Absorb;
        [SerializeReference, Reference()]
        private IDamageDef[] m_Resist;
        [SerializeField]
        private float m_Value;

        IReadOnlyCollection<IDamageDef> IPropertyDef.Absorb => m_Absorb;
        IReadOnlyCollection<IDamageDef> IPropertyDef.Resist => m_Resist;
        float IPropertyDef.Value => m_Value;

        protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            base.AddComponentData(entity, manager, conversionSystem);
        }
    }
}