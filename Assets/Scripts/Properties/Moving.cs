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
        [SerializeField]
        private float pointToPointThreshold = 0.005f;
        [SerializeField]
        private Waypoints m_Path;
        [SerializeField]
        private float m_ProgressDistance;
        [SerializeField]
        private Vector3 pathDynamicOffset;
        [SerializeField]
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
            var enemy = (unit as UnitEnemy);
            m_Path = enemy.SubWave.path;
            float dynamicX = Random.Range(-1f, 1f);
            float dynamicZ = Random.Range(-1f, 1f);
            pathDynamicOffset = new Vector3(dynamicX, 0, dynamicZ);
            Reset(enemy);
        }
        public void Update(Unit unit, float deltaTime)
        {
            if (!unit.Dead && m_Path != null)
            {
                if (MoveToPoint(unit, deltaTime))
                {
                    ReachDestination(unit);
                }
            }
        }
        //function call to rotate and move toward a pecific point, return true when the point is reached
        public bool MoveToPoint(Unit unit, float deltaTime)
        {
            var transform = unit.transform;
            m_ProgressDistance = m_ProgressDistance + (moveSpeed * deltaTime);
            Waypoints.RoutePoint progressPoint = m_Path.GetRoutePoint(m_ProgressDistance);
            Vector3 progressDelta = progressPoint.position - transform.position;
            transform.position = progressPoint.position + pathDynamicOffset;
            transform.rotation = Quaternion.LookRotation(progressPoint.direction);
            return (Vector3.Dot(endPoint.position - transform.position, endPoint.direction) < pointToPointThreshold) && m_ProgressDistance > m_Path.Length;
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
