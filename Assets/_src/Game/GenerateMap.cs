using System;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
//using UnityEditor;

namespace Game.Model.World
{
    public class GenerateMap : MonoBehaviour
    {
        public int2 Size;
        public int HeightMax = 10;
        public int TerrainOctaves = 6;
        public float TerrainFrequency = 1.25f;
        public int Seed = 12023;

        public void Execute(Action<Map.Data> callback)
        {
            StartCoroutine(MakeNewMap(callback));
        }

        IEnumerator MakeNewMap(Action<Map.Data> callback)
        {
            var manager = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;
            var q = manager.CreateEntityQuery(
                ComponentType.ReadWrite<Map.Data>()
            );
            manager.DestroyEntity(q);

            if (Seed == 0)
                Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

            var entity = manager.CreateEntity();
            manager.AddComponentData(entity, new Map.GenerateMapTag(
                TerrainOctaves,
                TerrainFrequency,
                Size,
                Seed,
                HeightMax,
                (p, s) => p.InitBusyTiles(s))
            );
            StartCoroutine(MakeMesh(callback));

            yield break;
        }

        IEnumerator MakeMesh(Action<Map.Data> callback)
        {
            var manager = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;

            var q = manager.CreateEntityQuery(
                ComponentType.ReadWrite<Map.Data>()
            );

            while (!Map.Singleton.IsInit())
            {
                yield return null;
            }

            var map = q.GetSingleton<Map.Data>();
            var obj = GetComponent<MapView>().InitMesh(map);
            map.InitView(obj.worldToLocalMatrix, obj.localToWorldMatrix, obj.GetComponent<MeshFilter>().sharedMesh.bounds);
            //UnityEditor.AssetDatabase.CreateAsset(obj.GetComponent<MeshFilter>().sharedMesh, "Assets/test.Mesh");
            callback?.Invoke(map);

            yield break;
        }


        private void OnDrawGizmos()
        {
            Rect r = new Rect(0, 0, Size.x, Size.y)
            {
                position = -new Vector2(Size.x, Size.y) * .5f
            };

            Vector3 pos = r.center;
            Vector3 size = new Vector3(Size.x, 1, Size.y);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(pos, size);
        }
    }
}
