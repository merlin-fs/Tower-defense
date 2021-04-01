using System;
using UnityEngine;

namespace Game.Entities.Damages
{
    public abstract class BaseDamage : IDamage
    {
        [SerializeField]
        private float m_Value;
        float IDamage.Value => m_Value;
    }
}
