using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[ExecuteAlways]
public class TestMain : MonoBehaviour
{
    [SerializeField]
    private Transform m_Object;

    private float3[] m_Points;
    private CatmullRomPath m_BezierPath;
    private float m_Time = 0;
    private PathInfo m_Info;


    
    private void Awake()
    {
        BuildPath();
    }

    void Start()
    {
    }

    [ContextMenu("rebuild")]
    private void BuildPath()
    {
        var list = new List<float3>();
        foreach (Transform iter in transform)
        {
            list.Add(iter.position);
        }
        m_Points = list.ToArray();

        m_BezierPath = new CatmullRomPath(m_Points);
        m_BezierPath.Closed = true;
        m_Info = SetTimeToLengthTables(m_BezierPath, 1000);
    }

    struct PathInfo
    {
        public float length;
        public float[] timesTable;
        public float[] lengthsTable;
        public float3[] drawPoints;
    }

    PathInfo SetTimeToLengthTables(CatmullRomPath p, int subdivisions)
    {
        float num = 0f;
        float num2 = 1f / (float)subdivisions;
        float[] array = new float[subdivisions];
        float[] array2 = new float[subdivisions];
        //p.GetPosition()
        float3 vector = p.GetPosition(0f);
        for (int i = 1; i < subdivisions + 1; i++)
        {
            float num3 = num2 * (float)i;
            float3 point = p.GetPosition(num3);
            num += math.distance(point, vector);
            vector = point;
            array[i - 1] = num3;
            array2[i - 1] = num;
        }
        return new PathInfo()
        {
            length = num,
            timesTable = array,
            lengthsTable = array2,
        };
    }


    static void RefreshNonLinearDrawWps(CatmullRomPath p, ref PathInfo info)
    {
        int num = p.ControlPoints.Count * 10;

        if (info.drawPoints == null || info.drawPoints.Length != num + 1)
        {
            info.drawPoints = new float3[num + 1];
        }
        for (int i = 0; i <= num; i++)
        {
            float perc = (float)i / (float)num;
            float3 point = p.GetPosition(perc);
            info.drawPoints[i] = point;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (!Application.isPlaying)
            return;

        m_Time += Time.deltaTime * 0.1f;
        if (m_Time > 1f)
            m_Time = 0;
    }

    private void OnDrawGizmos()
    {
        if (m_BezierPath == null)
            return;

        RefreshNonLinearDrawWps(m_BezierPath, ref m_Info);
        Draw(m_BezierPath, m_Info);
        //Gizmos.color = Color.red;
    }

    private static void Draw(CatmullRomPath p, PathInfo info)
    {
        if (info.timesTable == null)
        {
            return;
        }

        Color color = Color.yellow;
        color.a *= 0.5f;

        int num = p.ControlPoints.Count;

        {
            float3 vector = info.drawPoints[0];
            int num2 = info.drawPoints.Length;
            for (int j = 1; j < num2; j++)
            {
                float3 vector3 = info.drawPoints[j];
                Gizmos.DrawLine(vector3, vector);
                vector = vector3;
            }
        }
        Gizmos.color = color;
        for (int k = 0; k < num; k++)
        {
            Gizmos.DrawSphere(p.ControlPoints[k], 0.75f);
        }
        
        /*
        if (p.lookAtPosition != null)
        {
            Vector3 value = p.lookAtPosition.Value;
            Gizmos.DrawLine(p.targetPosition, value);
            Gizmos.DrawWireSphere(value, 0.075f);
        }
        */
    }

}
