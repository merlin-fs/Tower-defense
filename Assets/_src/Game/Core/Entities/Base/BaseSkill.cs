using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Entities
{
    [System.Serializable]
    public abstract class BaseSkill : BaseSlice, ISkill
    {

        [SerializeReference, SubclassSelector(typeof(IInfluence))]
        private List<IInfluence> m_Effects = new List<IInfluence>();
        public List<IInfluence> Effects { get => m_Effects; }
        public virtual void Init(IUnit unit) { }
        public virtual void Done(IUnit unit) { }
        public virtual void Update(IUnit unit, float deltaTime) { }
        protected virtual void ApplyEffects(IUnit sender, IUnit target)
        {
            if (target != null)
            {
                foreach (var effect in m_Effects)
                    effect.Apply(sender, target);
            }
        }
        public override void FillFrom(ISlice other)
        {
            if (other is BaseSkill skill)
            {
                m_Effects.Clear();
                foreach (var effect in skill.m_Effects)
                    m_Effects.Add(effect.Instantiate<IInfluence>());
            }
        }
    }
}
