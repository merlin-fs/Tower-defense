using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense
{
    [System.Serializable]
    public class Moving : ISkill
    {
        public float moveSpeed = 3;
        public float rotateSpeed = 10;
        public delegate void DestinationHandler(Unit unit);
        public static event DestinationHandler OnDestination;
        [SerializeField, HideInInspector]
        private float pointToPointThreshold = 0.05f;
        [SerializeField, HideInInspector]
        private Waypoints m_Path;
        [SerializeField, HideInInspector]
        private float m_ProgressDistance;
        [SerializeField, HideInInspector]
        private Vector3 pathDynamicOffset;
        [SerializeField, HideInInspector]
        private Waypoints.RoutePoint endPoint;
        ISlice ISlice.Clone()
        {
            return Clone();
        }
        public ISkill Clone()
        {
            return new Moving()
            {
                moveSpeed = this.moveSpeed,
                rotateSpeed = this.rotateSpeed,
                m_Path = this.m_Path,
            };
        }
        public void Init(Unit unit)
        {
            if (unit is UnitEnemy)
            {
                var enemy = (unit as UnitEnemy);
                m_Path = enemy.SubWave.path;
                float dynamicX = Random.Range(-1f, 1f);
                float dynamicZ = Random.Range(-1f, 1f);
                pathDynamicOffset = new Vector3(dynamicX, 0, dynamicZ);
                Reset(enemy);
            }
        }
        public void Update(Unit unit, float deltaTime)
        {
            if (!unit.Dead && m_Path != null)
            {
                // Если пересекли конечную точку (и пройденный путь больше длины пути)
                if (m_Path.Length - m_ProgressDistance <= pointToPointThreshold)
                {
                    ReachDestination(unit);
                }
            }
        }
        public void FixedUpdate(Unit unit, float deltaTime)
        {
            if (!unit.Dead && m_Path != null)
            {
                MoveToPoint(unit, deltaTime);
            }
        }
        /// <summary>
        /// Перемещает объект по пути m_Path(Waypoints)
        /// </summary>
        /// <param name="unit">Объект перемещения</param>
        /// <param name="deltaTime">ВременнАя дельта</param>
        /// <returns></returns>
        public void MoveToPoint(Unit unit, float deltaTime)
        {
            var transform = unit.transform;
            //Пройденное расстояние: скорость * время
            m_ProgressDistance += (moveSpeed * deltaTime);
            //Получам следующую точку из пути, по расстоянию
            Waypoints.RoutePoint progressPoint = m_Path.GetRoutePoint(m_ProgressDistance);
            Vector3 progressDelta = progressPoint.position - transform.position;
            //Устанавливаем новую позицию 
            transform.position = progressPoint.position + pathDynamicOffset;

            //Поворот в направлении точки, со скоростью rotateSpeed
            Quaternion wantedRot = Quaternion.LookRotation(progressPoint.direction);
            transform.rotation  = Quaternion.Slerp(transform.rotation, wantedRot, rotateSpeed * deltaTime);
        }
        private void Reset(Unit unit)
        {
            var transform = unit.transform;
            endPoint = m_Path.GetRoutePoint(m_Path.Length);
            m_ProgressDistance = 0;
            transform.position = m_Path.PointObjects[0].position + pathDynamicOffset;
            transform.rotation = m_Path.PointObjects[0].rotation;
        }
        private void ReachDestination(Unit unit)
        {
            OnDestination?.Invoke(unit);
            if (m_Path.loop)
            {
                Reset(unit);
                return;
            }
            float delay = 0;
            //if (aniInstance != null) { delay = aniInstance.PlayDestination(); }
            unit.DoDead(delay);
        }
    }
}
