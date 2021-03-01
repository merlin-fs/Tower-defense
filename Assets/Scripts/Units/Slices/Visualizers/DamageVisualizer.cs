using UnityEngine;
using St.Common.Core;

namespace TowerDefense.View
{
    using Core;
    using Core.View;

    // вместо IInfluence нужен конкретный тип
    public class DamageVisualizer : BaseVisualizer<IInfluence>
    {
        protected override void UpdateView(IUnit unit, ISlice slice, float deltaTime)
        {
            var parent = unit.TargetPoint;
            var go = Poolable.TryGetPoolable<ICoreGameObjectInstantiate>(gameObject).GameObject;

            go.transform.parent = parent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var system = go.GetComponent<ParticleSystem>();
            system.Play(true);
        }
    }
}