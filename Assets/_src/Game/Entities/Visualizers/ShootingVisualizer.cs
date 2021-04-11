using UnityEngine;

namespace Game.Entities.View
{
    public class ShootingVisualizer : BaseVisualizer<Shooting>
    {
        protected override void Init(IUnit unit)
        {
            var parent = unit.TargetPoint;
            transform.parent = parent;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        protected override void Done(IUnit unit)
        {

        }

        protected override void UpdateView(IUnit unit, ISlice slice, float deltaTime)
        {
            //Poolable.TryGetPoolable<ICoreGameObjectInstantiate>(gameObject).GameObject;
            if (unit.Turret != null)
                transform.localRotation = unit.Turret.GameObject.transform.localRotation;
            var system = GetComponent<ParticleSystem>();
            system.Play(true);
        }
    }
}