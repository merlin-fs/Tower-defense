using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Entities
{
    using St.Common.Core;
    using View;

    public interface IUnit: ICoreGameObjectInstantiate
    {
        void Init();
        void Done();

        IReadOnlyCollection<IProperty> Properties { get; }
        IReadOnlyCollection<ISkill> Skills { get; }
        IReadOnlyCollection<IInfluence> Influences { get; }

        void AddProperty(IProperty property);
        void AddSkill(ISkill skill);
        void AddInfluence(IInfluence influence);
        
        void RemoveProperty(IProperty property);
        void RemoveSkill(ISkill skill);
        void RemoveInfluence(IInfluence influence);

        bool IsDead { get; }
        void SetDead(float delay);

        ITurret Turret { get; }

        Transform TargetPoint { get; }
    }


    [Serializable]
    public class UnitContainer : TypedContainer<IUnit> { }


    public class Unit : MonoBehaviour, IUnit
    {
        public delegate void DamagedHandler(IUnit unit);
        //!!!public static event DamagedHandler OnDamaged;
        public delegate void DestroyedHandler(IUnit unit, float delay);

        private HashSet<IProperty> m_Properties = new HashSet<IProperty>();
        private HashSet<ISkill> m_Skills = new HashSet<ISkill>();
        private List<IInfluence> m_Influences = new List<IInfluence>();

        public static event DestroyedHandler OnDestroyed;

        public string unitName = "unit";
        public Sprite iconSprite;
        public string desp = "";

        [SerializeField]
        private Transform m_TargetPoint;
        [Serializable]
        private class TurretContainer : TypedContainer<ITurret> { }
        [SerializeField]
        private TurretContainer m_Turret;



        protected LayerMask maskTarget = 0;

        protected IUnit Self => this;
        #region IUnit
        void IUnit.Init()
        {
            Init();
        }

        void IUnit.Done()
        {
            Done();
        }

        IReadOnlyCollection<IProperty> IUnit.Properties => m_Properties;
        IReadOnlyCollection<ISkill> IUnit.Skills => m_Skills;
        IReadOnlyCollection<IInfluence> IUnit.Influences => m_Influences;

        void IUnit.AddProperty(IProperty property) => m_Properties.Add(property);
        void IUnit.AddSkill(ISkill skill) => m_Skills.Add(skill);
        void IUnit.AddInfluence(IInfluence influence) => m_Influences.Add(influence);

        void IUnit.RemoveProperty(IProperty property) => m_Properties.Remove(property);
        void IUnit.RemoveSkill(ISkill skill) => m_Skills.Remove(skill);
        void IUnit.RemoveInfluence(IInfluence influence) => m_Influences.Remove(influence);

        void IUnit.SetDead(float delay)
        {
            IsDead = true;
            //if (deadEffectObj != null)
            //    ObjectPoolManager.Spawn(deadEffectObj, GetTargetT().position, thisT.rotation);
            OnDestroyed.Invoke(this, delay);
        }

        Transform IUnit.TargetPoint => m_TargetPoint;

        ITurret IUnit.Turret => m_Turret.Value;
        public bool IsDead { get; private set; } = false;
        #endregion
        #region ICoreGameObjectInstantiate
        GameObject ICoreGameObject.GameObject => gameObject;

        T ICoreObjectInstantiate.Instantiate<T>()
        {
            IUnit clone = Instantiate(this);
            return (T)clone;
        }

        ICoreObjectInstantiate ICoreObjectInstantiate.Instantiate()
        {
            return Self.Instantiate<ICoreObjectInstantiate>();
        }
        void IDisposable.Dispose()
        {
            Destroy(gameObject);
        }
        #endregion

        public LayerMask GetTargetMask()
        {
            return maskTarget;
        }
        
        private void ClearInfluences()
        {
            m_Influences.Clear();
        }
        
        protected void InitSlice<T>(IEnumerable<T> list) where T : ISlice
        {
            foreach (T slice in list)
                (slice as ISliceInit)?.Init(this);
        }
        
        protected virtual void Init()
        {
            IsDead = false;
            InitSlice(m_Properties);
            InitSlice(m_Skills);
            ClearInfluences();
        }

        protected virtual void Done()
        {
        }

        private void Update()
        {
            foreach (var prop in m_Properties)
                prop.Update(this, Time.deltaTime);
            foreach (var skill in m_Skills)
                skill.Update(this, Time.deltaTime);
        }

        public virtual void FixedUpdate()
        {
            /*
            if (target != null && !IsInConstruction() && !stunned)
            {
				if (turretObject != null)
                {
					if (rotateTurretAimInXAxis && barrelObject != null)
                    {
						Vector3 targetPos = target.GetTargetT().position;
						Vector3 dummyPos = targetPos;
						dummyPos.y = turretObject.position.y;
						
						Quaternion wantedRot = Quaternion.LookRotation(dummyPos - turretObject.position);
						turretObject.rotation = Quaternion.Slerp(turretObject.rotation, wantedRot, turretRotateSpeed * Time.deltaTime);
						
						float angle = Quaternion.LookRotation(targetPos - barrelObject.position).eulerAngles.x;
						float distFactor = Mathf.Min(1, Vector3.Distance(turretObject.position, targetPos) / GetSOMaxRange());
						float offset = distFactor * GetSOMaxAngle();
						wantedRot = turretObject.rotation * Quaternion.Euler(angle - offset, 0, 0);
						
						barrelObject.rotation = Quaternion.Slerp(barrelObject.rotation, wantedRot, turretRotateSpeed * Time.deltaTime);
						targetInLOS = (Quaternion.Angle(barrelObject.rotation, wantedRot) < aimTolerance) ? true : false;
                    }
					else
                    {
						Vector3 targetPos = target.GetTargetT().position;
						if (!rotateTurretAimInXAxis)
                            targetPos.y = turretObject.position.y;
						
						Quaternion wantedRot = Quaternion.LookRotation(targetPos - turretObject.position);
						if (rotateTurretAimInXAxis)
                        {
							float distFactor = Mathf.Min(1, Vector3.Distance(turretObject.position, targetPos) / GetSOMaxRange());
							float offset = distFactor * GetSOMaxAngle();
							wantedRot *= Quaternion.Euler(-offset, 0, 0);
						}
						turretObject.rotation = Quaternion.Slerp(turretObject.rotation, wantedRot, turretRotateSpeed * Time.deltaTime);
						targetInLOS = (Quaternion.Angle(turretObject.rotation, wantedRot) < aimTolerance) ? true : false;
                    }
				}
				else
                    targetInLOS = true;
			}
			
			if (IsCreep() && target == null && turretObject != null && !stunned)
            {
				turretObject.localRotation = Quaternion.Slerp(
                    turretObject.localRotation, 
                    Quaternion.identity, 
                    turretRotateSpeed * Time.deltaTime * 0.25f);
			}
            */
        }
    }

}
