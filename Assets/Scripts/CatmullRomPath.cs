using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Unity.Mathematics;

//Contributed by Matthew Bettcher

/// <summary>
/// Path:
/// Very similar to Vertices, but this
/// class contains vectors describing
/// control points on a Catmull-Rom
/// curve.
/// </summary>
public class CatmullRomPath
{
    /// <summary>
    /// All the points that makes up the curve
    /// </summary>
    public List<float3> ControlPoints;

    private float _deltaT;

    /// <summary>
    /// Initializes a new instance of the <see cref="Path"/> class.
    /// </summary>
    public CatmullRomPath()
    {
        ControlPoints = new List<float3>();
    }

    public CatmullRomPath(IEnumerable<float3> vertices)
    {
        int count = vertices.Count();
        ControlPoints = new List<float3>(count);
        foreach (var iter in vertices)
        {
            Add(iter);
        }
    }

    /// <summary>
    /// True if the curve is closed.
    /// </summary>
    /// <value><c>true</c> if closed; otherwise, <c>false</c>.</value>
    public bool Closed { get; set; }

    /// <summary>
    /// Gets the next index of a controlpoint
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns></returns>
    public int NextIndex(int index)
    {
        if (index == ControlPoints.Count - 1)
        {
            return 0;
        }
        return index + 1;
    }

    /// <summary>
    /// Gets the previous index of a controlpoint
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns></returns>
    public int PreviousIndex(int index)
    {
        if (index == 0)
        {
            return ControlPoints.Count - 1;
        }
        return index - 1;
    }

    /// <summary>
    /// Translates the control points by the specified vector.
    /// </summary>
    /// <param name="vector">The vector.</param>
    public void Translate(ref float3 vector)
    {
        for (int i = 0; i < ControlPoints.Count; i++)
            ControlPoints[i] += vector;
    }

    /// <summary>
    /// Scales the control points by the specified vector.
    /// </summary>
    /// <param name="value">The Value.</param>
    public void Scale(ref float3 value)
    {
        for (int i = 0; i < ControlPoints.Count; i++)
            ControlPoints[i] *= value;
    }

    /// <summary>
    /// Rotate the control points by the defined value in radians.
    /// </summary>
    /// <param name="value">The amount to rotate by in radians.</param>
    /*
    public void Rotate(float value)
    {
        Matrix4x4 rotationMatrix = Matrix4x4.CreateRotationZ(value);

        for (int i = 0; i < ControlPoints.Count; i++)
            ControlPoints[i] = float2.Transform(ControlPoints[i], rotationMatrix);
    }
    */

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < ControlPoints.Count; i++)
        {
            builder.Append(ControlPoints[i].ToString());
            if (i < ControlPoints.Count - 1)
            {
                builder.Append(" ");
            }
        }
        return builder.ToString();
    }

    /// <summary>
    /// Returns a set of points defining the
    /// curve with the specifed number of divisions
    /// between each control point.
    /// </summary>
    /// <param name="divisions">Number of divisions between each control point.</param>
    /// <returns></returns>

    public IList<float3> GetVertices(int divisions)
    {
        var verts = new List<float3>();

        float timeStep = 1f / divisions;

        for (float i = 0; i < 1f; i += timeStep)
        {
            verts.Add(GetPosition(i));
        }

        return verts;
    }


    public float3 GetPosition(float time)
    {
        float3 temp;

        if (ControlPoints.Count < 2)
            throw new Exception("You need at least 2 control points to calculate a position.");

        if (Closed)
        {
            Add(ControlPoints[0]);

            _deltaT = 1f / (ControlPoints.Count - 1);

            int p = (int)(time / _deltaT);

            // use a circular indexing system
            int p0 = p - 1;
            if (p0 < 0) p0 = p0 + (ControlPoints.Count - 1);
            else if (p0 >= ControlPoints.Count - 1) p0 = p0 - (ControlPoints.Count - 1);
            int p1 = p;
            if (p1 < 0) p1 = p1 + (ControlPoints.Count - 1);
            else if (p1 >= ControlPoints.Count - 1) p1 = p1 - (ControlPoints.Count - 1);
            int p2 = p + 1;
            if (p2 < 0) p2 = p2 + (ControlPoints.Count - 1);
            else if (p2 >= ControlPoints.Count - 1) p2 = p2 - (ControlPoints.Count - 1);
            int p3 = p + 2;
            if (p3 < 0) p3 = p3 + (ControlPoints.Count - 1);
            else if (p3 >= ControlPoints.Count - 1) p3 = p3 - (ControlPoints.Count - 1);

            // relative time
            float lt = (time - _deltaT * p) / _deltaT;

            temp = CatmullRom(ControlPoints[p0], ControlPoints[p1], ControlPoints[p2], ControlPoints[p3], lt);

            RemoveAt(ControlPoints.Count - 1);
        }
        else
        {
            int p = (int)(time / _deltaT);

            // 
            int p0 = p - 1;
            if (p0 < 0) p0 = 0;
            else if (p0 >= ControlPoints.Count - 1) p0 = ControlPoints.Count - 1;
            int p1 = p;
            if (p1 < 0) p1 = 0;
            else if (p1 >= ControlPoints.Count - 1) p1 = ControlPoints.Count - 1;
            int p2 = p + 1;
            if (p2 < 0) p2 = 0;
            else if (p2 >= ControlPoints.Count - 1) p2 = ControlPoints.Count - 1;
            int p3 = p + 2;
            if (p3 < 0) p3 = 0;
            else if (p3 >= ControlPoints.Count - 1) p3 = ControlPoints.Count - 1;

            // relative time
            float lt = (time - _deltaT * p) / _deltaT;

            temp = CatmullRom(ControlPoints[p0], ControlPoints[p1], ControlPoints[p2], ControlPoints[p3], lt);
        }

        return temp;
    }

    /// <summary>
    /// Gets the normal for the given time.
    /// </summary>
    /// <param name="time">The time</param>
    /// <returns>The normal.</returns>
    public float3 GetPositionNormal(float time)
    {
        float offsetTime = time + 0.0001f;

        float3 a = GetPosition(time);
        float3 b = GetPosition(offsetTime);

        float3 output, temp;

        temp = a - b;

#if (XBOX360 || WINDOWS_PHONE)
output = new float2();
#endif
        output.x = -temp.y;
        output.y = temp.x;
        output.z = temp.z;

        if (math.all(output != float3.zero))
            output = math.normalize(output);


        return output;
    }

    public void Add(float3 point)
    {
        ControlPoints.Add(point);
        _deltaT = 1f / (ControlPoints.Count - 1);
    }

    public void Remove(float3 point)
    {
        ControlPoints.Remove(point);
        _deltaT = 1f / (ControlPoints.Count - 1);
    }

    public void RemoveAt(int index)
    {
        ControlPoints.RemoveAt(index);
        _deltaT = 1f / (ControlPoints.Count - 1);
    }

    public float GetLength()
    {
        var verts = GetVertices(ControlPoints.Count * 25);
        float length = 0;

        for (int i = 1; i < verts.Count; i++)
        {
            length += math.distance(verts[i - 1], verts[i]);
        }

        if (Closed)
            length += math.distance(verts[ControlPoints.Count - 1], verts[0]);

        return length;
    }

    /*
    public List<float3> SubdivideEvenly(int divisions)
    {
        List<float3> verts = new List<float3>();

        float length = GetLength();

        float deltaLength = length / divisions + 0.001f;
        float t = 0.000f;

        // we always start at the first control point
        float3 start = ControlPoints[0];
        float3 end = GetPosition(t);

        // increment t until we are at half the distance
        while (deltaLength * 0.5f >= math.distance(start, end))
        {
            end = GetPosition(t);
            t += 0.0001f;

            if (t >= 1f)
                break;
        }

        start = end;

        // for each box
        for (int i = 1; i < divisions; i++)
        {
            float3 normal = GetPositionNormal(t);
            float angle = (float)Math.Atan2(normal.y, normal.x);

            verts.Add(new float3(end, angle));

            // until we reach the correct distance down the curve
            while (deltaLength >= math.distance(start, end))
            {
                end = GetPosition(t);
                t += 0.00001f;

                if (t >= 1f)
                    break;
            }
            if (t >= 1f)
                break;

            start = end;
        }
        return verts;
    }
    */

    public static float3 CatmullRom(float3 value1, float3 value2, float3 value3, float3 value4, float amount)
    {
        return new float3(
            CatmullRom(value1.x, value2.x, value3.x, value4.x, amount),
            CatmullRom(value1.y, value2.y, value3.y, value4.y, amount),
            CatmullRom(value1.z, value2.z, value3.z, value4.z, amount));
    }

    public static void CatmullRom(ref float3 value1, ref float3 value2, ref float3 value3, ref float3 value4,
                                  float amount, out float3 result)
    {
        result = new float3(
            CatmullRom(value1.x, value2.x, value3.x, value4.x, amount),
            CatmullRom(value1.y, value2.y, value3.y, value4.y, amount),
            CatmullRom(value1.z, value2.z, value3.z, value4.z, amount));
    }

    /*
    public static void Transform(ref float2 position, ref Matrix4x4 matrix, out float2 result)
    {
        result = new float2((position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M41,
                             (position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M42);
    }
    */

    /*
    public static void Transform(float2[] sourceArray, ref Matrix4x4 matrix, float2[] destinationArray)
    {
        for (int i = 0; i < sourceArray.Length; i++)
        {
            Transform(ref sourceArray[i], ref matrix, out destinationArray[i]);
        }
    }
    */

    public static float CatmullRom(float value1, float value2, float value3, float value4, float amount)
    {
        // Using formula from http://www.mvps.org/directx/articles/catmull/
        // Internally using doubles not to lose precission
        double amountSquared = amount * amount;
        double amountCubed = amountSquared * amount;
        return (float)(0.5 * (2.0 * value2 +
                             (value3 - value1) * amount +
                             (2.0 * value1 - 5.0 * value2 + 4.0 * value3 - value4) * amountSquared +
                             (3.0 * value2 - value1 - 3.0 * value3 + value4) * amountCubed));
    }

}
