using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model.World
{
    public partial struct Map
    {
        public float3 MapToWord(int2 value, float height = 0f)
        {
            float3 pos = new float3(value.x + 0.5f, height, value.y + 0.5f);
            return math.transform(ViewData.LocalToWorldMatrix, pos);
        }

        public float3 MapToWord(int2 value)
        {
            int idx = this.At(value);
            var height = Mathf.Lerp(ViewData.Bounds.min.y, ViewData.Bounds.max.y + 0.5f, Tiles.Heights[idx].Value) + ViewData.Bounds.min.y;
            //height += 8f;

            float3 pos = new float3(value.x + 0.5f, height, value.y + 0.5f);
            pos = math.transform(ViewData.LocalToWorldMatrix, pos);
            return pos;
        }
    }
}
