using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model.World
{
    public partial class Map
    {
        public struct HeightType: IBufferElementData
        {
            public enum @Type
            {
                /// <summary>
                /// Глубокая вода
                /// </summary>
                DeepWater = 1,
                /// <summary>
                /// Мелководье
                /// </summary>
                ShallowWater = 2,
                /// <summary>
                /// Берег
                /// </summary>
                Shore = 3,
                /// <summary>
                /// Песок
                /// </summary>
                Sand = 4,
                /// <summary>
                /// Трава
                /// </summary>
                Grass = 5,
                /// <summary>
                /// Лес
                /// </summary>
                Forest = 6,
                /// <summary>
                /// Камни
                /// </summary>
                Rock = 7,
                /// <summary>
                /// Снег
                /// </summary>
                Snow = 8
            }

            public Type Value;
            public static implicit operator HeightType(Type value) => new HeightType { Value = value };
        }
    }
}
