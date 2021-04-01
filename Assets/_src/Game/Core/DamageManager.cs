using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    using Entities;

    public interface IDamageManager
    {
        void Damage(IUnit sender, IUnit target, IDamage damage);
    }

    public class DamageManager : MonoBehaviour, IDamageManager
    {
        [SerializeField]
        List<Type> m_PropertyPriority = new List<Type>();

        private void Awake()
        {
            Root.Bind<IDamageManager>(this);
            m_PropertyPriority.Add(typeof(Shield));
            m_PropertyPriority.Add(typeof(Health));
        }

        void IDamageManager.Damage(IUnit sender, IUnit target, IDamage damage)
        {
            Type damageType = damage.GetType();
            float value = damage.Value;
            
            value = ApplyBoost(sender, value);
            ProcessProperties(sender, target, damageType, value);
        }

        private float ApplyBoost(IUnit unit, float value)
        {
            return value;
        }

        private void ProcessProperties(IUnit sender, IUnit target, Type damageType, float value)
        {
            foreach(var iter in m_PropertyPriority)
            {
                IProperty property = GetPropertyFromType(iter, target.Properties);
                if (property != null && property.Value > 0)
                {
                    //Вычисление урона (damage = value * resist)
                    float damage = value;
                    IDamage resist = GetDamageFromType(damageType, property.Resist);
                    damage *= (resist?.Value ?? 1);
                    property.AddDamage(sender, damage);

                    //Вычисление поглощения (in value *= 1 - absorb)
                    IDamage absorb = GetDamageFromType(damageType, property.Absorb);
                    value *= 1 - (absorb?.Value ?? 1);
                }
            }
        }

        private IProperty GetPropertyFromType(Type type, IReadOnlyCollection<IProperty> list)
        {
            foreach (var iter in list)
            {
                if (iter.GetType() == type)
                    return iter;
            }
            return null;
        }
        private IDamage GetDamageFromType(Type type, IReadOnlyCollection<IDamage> list)
        {
            foreach (var iter in list)
            {
                if (iter.GetType() == type)
                    return iter;
            }
            return null;
        }
    }
}