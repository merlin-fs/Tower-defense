using System;
using System.Collections.Generic;
using UnityEngine;
using St.Common.Core;

namespace Game.Entities
{
    using View;

    [System.Serializable]
    public abstract class BaseSkill<T> : BaseSlice, ISkill
        where T : BaseSkill<T>
   {
        [Serializable]
        private class VisualizerContainer : TypedContainer<ISliceVisualizer<T>> { }
        [SerializeField]
        private VisualizerContainer m_ViewPrefab = new VisualizerContainer();

        [SerializeReference, SubclassSelector(typeof(IInfluence))]
        private List<IInfluence> m_Effects = new List<IInfluence>();

        private ISliceVisualizer<T> m_View;

        protected IUnit Owner { get; private set; }
        protected ISliceVisualizer<T> View => m_View;

        public List<IInfluence> Effects { get => m_Effects; }
        public virtual void Init(IUnit unit) 
        {
            Owner = unit;
            m_View = m_ViewPrefab.Value is ICoreInstantiate inst && m_ViewPrefab.Value is ICoreMonoObject obj && obj.GameObject.IsPrefab()
                ? inst.Instantiate<ISliceVisualizer<T>>()
                : m_ViewPrefab.Value;
            View?.Init(unit);

            foreach (var iter in m_Effects)
                iter.Init(unit);
        }

        public virtual void Done(IUnit unit) 
        {
            foreach (var iter in m_Effects)
                iter.Done(unit);
            View?.Done(unit);
            if (m_ViewPrefab.Value is ICoreMonoObject obj && obj.GameObject.IsPrefab()
                && m_View is ICoreDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        public virtual void Update(IUnit unit, float deltaTime) { }
       
        protected virtual void ApplyEffects(IUnit sender, IUnit target, float deltaTime)
        {
            if (target != null)
            {
                foreach (var effect in m_Effects)
                {
                    target.AddInfluence(effect);
                    effect.Activate(sender, target, deltaTime);
                }
            }
        }

        public override void FillFrom(ISlice other)
        {
            if (other is BaseSkill<T> skill)
            {
                m_ViewPrefab = skill.m_ViewPrefab;
                m_Effects = new List<IInfluence>(skill.m_Effects);
            }
        }
    }
}
