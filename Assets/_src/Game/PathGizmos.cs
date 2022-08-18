using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Game.Model.Units;

namespace Game.Model.World
{
    public class PathGizmos : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            StartCoroutine(DrawGizmos());
        }

        IEnumerator DrawGizmos()
        {
            if (Unity.Entities.World.DefaultGameObjectInjectionWorld == null)
                yield break;

            var manager = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;
            var q = manager.CreateEntityQuery(
                ComponentType.ReadOnly<Map.Path.Info>(),
                ComponentType.ReadOnly<Map.Path.Points>(),
                ComponentType.ReadOnly<Map.Path.Times>()
            );

            while (q.CalculateEntityCount() <= 0)
            {
                yield return null;
            }

            var entities = q.ToEntityArray(Allocator.Temp);
            for (var i = 0; i < entities.Length; i++)
            {
                var path = manager.GetComponentData< Map.Path.Info>(entities[i]);
                var points = manager.GetBuffer<Map.Path.Points>(entities[i]);
                if (points.Length == 0)
                    continue;
                try
                {
                    GL.PushMatrix();
                    GL.LoadOrtho();
                    float3 point = Map.Path.GetPosition(0, false, points.AsNativeArray(), path.DeltaTime);
                    for (float t = 0; t < 1; t += 1f / 200)
                    {
                        float3 next = Map.Path.GetPosition(t, false, points.AsNativeArray(), path.DeltaTime);
                        Gizmos.DrawLine(point, next);
                        point = next;
                    }
                    GL.PopMatrix();
                }
                finally
                {
                }
            }
            entities.Dispose();
        }


    }
}
