using System;
using System.Collections;

using UnityEngine;

    #if UNITY_EDITOR
    using UnityEditor;
    #endif

namespace TowerDefense.Core
{

    public interface IWaypoints
    {
        bool Loop { get; }
        float Length { get; }
        float GetLengthLerp(float t);
        Vector3 GetPosition(float length);
        (Vector3 Position, Vector3 Direction) GetRoutePoint(float length);
    }


    public class Waypoints : MonoBehaviour, IWaypoints
    {
        [Serializable]
        public struct PointObject
        {
            public int Length => Objects.Length;
            public Transform[] Objects;
            public Transform this[int idx] => Objects[idx];
        }

        [SerializeField]
        private bool m_SmoothRoute = true;
        [SerializeField]
        private bool m_Loop = false;
        [SerializeField]
        private float m_EditorVisualisationSubsteps = 100;
        [SerializeField]
        private PointObject m_PointObjects = new PointObject { Objects = new Transform[] { } };

        private Vector3[] m_Points;
        private float[] m_Distances;
        private float m_Length;

        IWaypoints Self => this;


        private void Awake()
        {
            Rebuild();
        }

        #region IWaypoints
        bool IWaypoints.Loop => m_Loop;

        float IWaypoints.Length => m_Length;

        float IWaypoints.GetLengthLerp(float t)
        {
            return Mathf.Lerp(0f, m_Length, t);
        }

        (Vector3 Position, Vector3 Direction) IWaypoints.GetRoutePoint(float length)
        {
            // position and direction
            Vector3 p1 = GetRoutePosition(length);
            Vector3 p2 = GetRoutePosition(length + 0.1f);
            Vector3 delta = p2 - p1;
            return (p1, delta.normalized);
        }

        Vector3 IWaypoints.GetPosition(float length)
        {
            return GetRoutePosition(length);
        }
        #endregion

        public void Rebuild()
        {
            m_Points = null;
            m_Distances = null;
            m_Length = 0;
            if (m_PointObjects.Length > 1)
                BuildPath();
        }

        public Vector3 GetRoutePosition(float dist)
        {
            int point = 0;
            dist = Mathf.Repeat(dist, m_Distances[m_Distances.Length - 1]);

            while (m_Distances[point] < dist)
                ++point;

            // get nearest two points, ensuring points wrap-around start & end of circuit
            int idx1 = (point - 1 + m_Points.Length) % m_Points.Length;
            int idx2 = point;

            // found point numbers, now find interpolation value between the two middle points
            float i = Mathf.InverseLerp(m_Distances[idx1], m_Distances[idx2], dist);

            if (m_SmoothRoute)
            {
                // smooth catmull-rom calculation between the two relevant points

                // get indices for the surrounding 2 points, because
                // four points are required by the catmull-rom function
                int idx0 = (point - 2 + m_Points.Length) % m_Points.Length;
                int idx3 = (point + 1) % m_Points.Length;

                // 2nd point may have been the 'last' point - a dupe of the first,
                // (to give a value of max track distance instead of zero)
                // but now it must be wrapped back to zero if that was the case.
                idx2 %= m_Points.Length;

                return CatmullRom(m_Points[idx0], m_Points[idx1], m_Points[idx2], m_Points[idx3], i);
            }
            else
            {
                // simple linear lerp between the two points:
                return Vector3.Lerp(m_Points[idx1], m_Points[idx2], i);
            }
        }


        private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float i)
        {
            // catmull-rom equation.
            return 0.5f *
                    ((2 * p1) + (-p0 + p2) * i + (2 * p0 - 5 * p1 + 4 * p2 - p3) * i * i +
                    (-p0 + 3 * p1 - 3 * p2 + p3) * i * i * i);
        }

        private void BuildPath()
        {
            // transfer the position of each point and distances between points to arrays for

            m_Points = new Vector3[m_PointObjects.Length + 1];
            m_Distances = new float[m_PointObjects.Length + 1];

            float accumulateDistance = 0;
            for (int i = 0; i < m_Points.Length; ++i)
            {
                Vector3 p1 = m_PointObjects[i % m_PointObjects.Length].position;
                Vector3 p2 = m_PointObjects[(i + 1) % m_PointObjects.Length].position;

                m_Points[i] = p1;
                m_Distances[i] = accumulateDistance;
                accumulateDistance += (p1 - p2).magnitude;
            }
            int idx = (m_Loop)
                ? (m_Distances.Length - 1)
                : (m_Distances.Length - 2 + m_Distances.Length) % m_Distances.Length;
            m_Length = m_Distances[idx];
        }


        private void OnDrawGizmos()
        {
            DrawGizmos(false);
        }


        private void OnDrawGizmosSelected()
        {
            DrawGizmos(true);
        }


        private void DrawGizmos(bool selected)
        {
#if UNITY_EDITOR
            Rebuild();
#endif
            if (m_PointObjects.Length > 1)
            {
                Gizmos.color = selected ? Color.yellow : new Color(1, 1, 0, 0.5f);
                Vector3 prev = m_PointObjects[0].position;
                if (m_SmoothRoute)
                {
                    for (float dist = 0; dist < Self.Length; dist += Self.Length / m_EditorVisualisationSubsteps)
                    {
                        Vector3 next = GetRoutePosition(dist + 1);
                        Gizmos.DrawLine(prev, next);
                        prev = next;
                    }
                    if (m_Loop)
                        Gizmos.DrawLine(prev, m_PointObjects[0].position);
                }
                else
                {
                    for (int n = m_Loop ? 0 : 1; n < m_PointObjects.Length; ++n)
                    {
                        Vector3 next = (m_Loop)
                            ? m_PointObjects[(n + 1) % m_PointObjects.Length].position
                            : m_PointObjects[n].position;
                        Gizmos.DrawLine(prev, next);
                        prev = next;
                    }
                }
            }
        }
    }
}
