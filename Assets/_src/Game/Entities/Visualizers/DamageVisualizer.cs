using UnityEngine;

namespace Game.Entities.View
{
    // вместо IInfluence нужен конкретный тип
    public class DamageVisualizer : BaseVisualizer<IInfluence>
    {
        protected override void UpdateView(IUnit unit, ISlice slice, float deltaTime)
        {
            var parent = unit.TargetPoint;

            //Poolable.TryGetPoolable<ICoreGameObjectInstantiate>(gameObject).GameObject;
            var go = Instantiate(gameObject);
            go.transform.parent = parent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            if (unit.Turret != null)
                go.transform.localRotation = unit.Turret.GameObject.transform.localRotation;

            var system = go.GetComponent<ParticleSystem>();
            system.Play(true);
        }
    }
}