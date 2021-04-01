using UnityEngine;

namespace Game.Entities
{
    using Core;

    [System.Serializable]
    public class Moving : BaseSkill
    {
        public float moveSpeed = 3;
        public float rotateSpeed = 10;
        public delegate void DestinationHandler(IUnit unit);
        public static event DestinationHandler OnDestination;
        [SerializeField, HideInInspector]
        private float pointToPointThreshold = 0.05f;
        [SerializeField, HideInInspector]
        private IWaypoints m_Path;
        [SerializeField, HideInInspector]
        private float m_ProgressDistance;
        [SerializeField, HideInInspector]
        private Vector3 pathDynamicOffset;
        public override void Init(IUnit unit)
        {
            if (unit is UnitEnemy)
            {
                var enemy = (unit as UnitEnemy);
                m_Path = enemy.SubWave.path.Value;
                float dynamicX = Random.Range(-1f, 1f);
                float dynamicZ = Random.Range(-1f, 1f);
                pathDynamicOffset = new Vector3(dynamicX, 0, dynamicZ);
                Reset(enemy);
            }
        }
        public override void Done(IUnit unit) { }

        public override void Update(IUnit unit, float deltaTime)
        {
            if (!unit.IsDead && m_Path != null)
            {
                MoveToPoint(unit, deltaTime);
                // Если пересекли конечную точку (и пройденный путь больше длины пути)
                if (m_Path.Length - m_ProgressDistance <= pointToPointThreshold)
                {
                    ReachDestination(unit);
                }
            }
        }

        public override void FillFrom(ISlice other)
        {
            base.FillFrom(other);
            if (other is Moving)
            {
                var moving = (other as Moving);
                moveSpeed = moving.moveSpeed;
                rotateSpeed = moving.rotateSpeed;
                pointToPointThreshold = moving.pointToPointThreshold;
                m_Path = moving.m_Path;
            }
        }
        /// <summary>
        /// Перемещает объект по пути m_Path(Waypoints)
        /// </summary>
        /// <param name="unit">Объект перемещения</param>
        /// <param name="deltaTime">ВременнАя дельта</param>
        /// <returns></returns>
        public void MoveToPoint(IUnit unit, float deltaTime)
        {
            //Пройденное расстояние: скорость * время
            m_ProgressDistance += (moveSpeed * deltaTime);
            //Получам следующую точку из пути, по расстоянию
            (Vector3 pos, Vector3 dir) = m_Path.GetRoutePoint(m_ProgressDistance);

            //Устанавливаем новую позицию 
            unit.GameObject.transform.position = pos + pathDynamicOffset;

            //Поворот в направлении точки, со скоростью rotateSpeed
            Quaternion wantedRot = Quaternion.LookRotation(dir);
            unit.GameObject.transform.rotation = Quaternion.Slerp(unit.GameObject.transform.rotation, wantedRot, rotateSpeed * deltaTime);
        }

        private void Reset(IUnit unit)
        {
            m_ProgressDistance = 0;
            (Vector3 pos, Vector3 dir) = m_Path.GetRoutePoint(m_ProgressDistance);
            unit.GameObject.transform.position = pos + pathDynamicOffset;
            unit.GameObject.transform.rotation = Quaternion.LookRotation(dir);
        }

        private void ReachDestination(IUnit unit)
        {
            var target = GameObject.FindObjectOfType<UserBase>();
            ApplyEffects(unit, target);
            OnDestination?.Invoke(unit);
            if (m_Path.Loop)
            {
                Reset(unit);
                return;
            }
            float delay = 0;
            //if (aniInstance != null) { delay = aniInstance.PlayDestination(); }
            unit.SetDead(delay);
        }
    }
}
