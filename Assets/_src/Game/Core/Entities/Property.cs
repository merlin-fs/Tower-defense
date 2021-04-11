using System;

namespace Game.Entities
{
    /// <summary>
    /// Свойства объекта (Health, shield, armor и т.п.)
    /// </summary>
    public interface IProperty : ISlice, ISliceInit, ISliceUpdate, IDamaged
    {
        float Value { get; }
        float Normalize { get; }
    }
}