using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace TowerDefense.Core
{
    public class TypeConstraintAttribute : PropertyAttribute
    {
        private System.Type type;

        public TypeConstraintAttribute(System.Type type)
        {
            this.type = type;
        }

        public System.Type Type
        {
            get { return type; }
        }
    }
    public static class PropertyDB
    {
        public static IProperty[] Properties = new IProperty[] { new Health(), new Shield() };
        public static ISkill[] Skills = new ISkill[] { new Moving() };
        public static IInfluence[] Influences = new IInfluence[] { };
        public static string[] GetSkills()
        {
            return Skills.Select(skill => skill.GetType().Name).ToArray();
        }
    }
    public class Unit : MonoBehaviour
    {
        public delegate void DamagedHandler(Unit unit);
        //!!!public static event DamagedHandler OnDamaged;

        public delegate void DestroyedHandler(Unit unit, float delay);
        public static event DestroyedHandler OnDestroyed;

        public string unitName = "unit";
        public Sprite iconSprite;
        public string desp = "";
        public Transform targetPoint;
        [SerializeReference, SerializeReferenceButton]
        public List<IProperty> Properties = new List<IProperty>();
        [SerializeReference, SerializeReferenceButton]
        public List<ISkill> Skills = new List<ISkill>();
        [HideInInspector, SerializeReference]//, SerializeReferenceButton
        public List<IInfluence> Influences = new List<IInfluence>();
        protected LayerMask maskTarget = 0;
        public bool Dead { get; private set; } = false;
        public LayerMask GetTargetMask()
        {
            return maskTarget;
        }
        private void ClearInfluences()
        {
            Influences.Clear();
        }
        protected void InitSlice<T>(IEnumerable<T> list) where T: ISlice
        {
            foreach (T slice in list)
                slice.Init(this);
        }
        public virtual void Awake()
        {
            
        }
		public virtual void Init()
        {
            Dead = false;
            InitSlice(Properties);
            InitSlice(Skills);
            ClearInfluences();
        }
		public virtual void Start()
        {
		}
		public virtual void OnEnable()
        {
		}
		public virtual void OnDisable()
        {
		}
		public virtual void Update()
        {
		}
		public virtual void FixedUpdate()
        {
            foreach (var prop in Properties)
            {
                prop.Update(this, Time.fixedDeltaTime);
            }
            foreach (var skill in Skills)
            {
                skill.Update(this, Time.fixedDeltaTime);
            }
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
        public void DoDead(float delay)
        {
			Dead = true;
			//if (deadEffectObj != null)
            //    ObjectPoolManager.Spawn(deadEffectObj, GetTargetT().position, thisT.rotation);
            OnDestroyed.Invoke(this, delay);
		}
		void OnDrawGizmos()
        {
            /*
            if (target!=null)
            {
				if (IsCreep())
                    Gizmos.DrawLine(transform.position, target.transform.position);
			}
            */
		}
	}

}
