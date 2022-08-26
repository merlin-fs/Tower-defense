using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

namespace Game.Model.World
{
    public partial class Map
    {
        public partial struct Path
        {
            public struct Info : IComponentData
            {
                public float Length;
                public float DeltaTime;
            }

            public struct Points : IBufferElementData
            {
                public float3 Value;
                public static implicit operator Points(float3 value) => new Points { Value = value };
            }

            public struct Times : IBufferElementData
            {
                public float Time;
                public float Length;
            }

            public static float3 GetPosition(float time, bool isClosed, [ReadOnly] NativeArray<Points> points, float deltaTime)
            {
                if (points.Length < 2)
                    throw new Exception("You need at least 2 control points to calculate a position.");

                if (isClosed)
                {

                    int p = (int)(time / deltaTime);

                    // use a circular indexing system
                    int p0 = p - 1;
                    if (p0 < 0) p0 = p0 + (points.Length - 1);
                    else if (p0 >= points.Length - 1) p0 = p0 - (points.Length - 1);
                    int p1 = p;
                    if (p1 < 0) p1 = p1 + (points.Length - 1);
                    else if (p1 >= points.Length - 1) p1 = p1 - (points.Length - 1);
                    int p2 = p + 1;
                    if (p2 < 0) p2 = p2 + (points.Length - 1);
                    else if (p2 >= points.Length - 1) p2 = p2 - (points.Length - 1);
                    int p3 = p + 2;
                    if (p3 < 0) p3 = p3 + (points.Length - 1);
                    else if (p3 >= points.Length - 1) p3 = p3 - (points.Length - 1);

                    // relative time
                    float lt = (time - deltaTime * p) / deltaTime;

                    return CatmullRom(points[p0].Value, points[p1].Value, points[p2].Value, points[p3].Value, lt);
                }
                else
                {
                    int p = (int)(time / deltaTime);

                    // 
                    int p0 = p - 1;
                    if (p0 < 0) p0 = 0;
                    else if (p0 >= points.Length - 1) p0 = points.Length - 1;
                    int p1 = p;
                    if (p1 < 0) p1 = 0;
                    else if (p1 >= points.Length - 1) p1 = points.Length - 1;
                    int p2 = p + 1;
                    if (p2 < 0) p2 = 0;
                    else if (p2 >= points.Length - 1) p2 = points.Length - 1;
                    int p3 = p + 2;
                    if (p3 < 0) p3 = 0;
                    else if (p3 >= points.Length - 1) p3 = points.Length - 1;

                    // relative time
                    float lt = (time - deltaTime * p) / deltaTime;
                    return CatmullRom(points[p0].Value, points[p1].Value, points[p2].Value, points[p3].Value, lt);
                    //return math.clamp(lt, points[p1].Value, points[p2].Value);
                }
            }

            public static float ConvertToConstantPathTime(float time, float length, NativeArray<Times> times)
            {
                if (time > 0f && time <= 1f)
                {
                    if (length <= 0f)
                    {
                        return time;
                    }
                    float target = length * time;
                    
                    float prevTime = 0f;
                    float prevLen = 0f;
                    float currentTime = 0f;
                    float currentLen = 0f;
                    for (int i = 0; i < times.Length; i++)
                    {
                        if (times[i].Length > target)
                        {
                            currentTime = times[i].Time;
                            currentLen = times[i].Length;
                            break;
                        }
                        else
                        {
                            prevLen = times[i].Length;
                            prevTime = times[i].Time;
                        }
                    }
                    time = prevTime + (target - prevLen) / (currentLen - prevLen) * (currentTime - prevTime);
                }
                if (time > 1f)
                {
                    time = 1f;
                }
                else if (time < 0f)
                {
                    time = 0f;
                }
                return time;
            }


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

            public static float3 CatmullRom(float3 value1, float3 value2, float3 value3, float3 value4, float amount)
            {
                return new float3(
                    CatmullRom(value1.x, value2.x, value3.x, value4.x, amount),
                    CatmullRom(value1.y, value2.y, value3.y, value4.y, amount),
                    CatmullRom(value1.z, value2.z, value3.z, value4.z, amount));
            }

        }
    }
}