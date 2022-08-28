using System;
using UnityEngine;
using Unity.Entities;
using Common.Defs;
using Common.Core;

namespace Game.Model.Damages
{
    public interface IDamageDef : IDef
    {
        float Value { get; }
    }

   public abstract class BaseDamageDef<T> : ClassDef<T>, IDamageDef
        where T : struct, IDefineable, IComponentData
    {
        [SerializeField]
        float m_Value;

        float IDamageDef.Value => m_Value;
    }
}