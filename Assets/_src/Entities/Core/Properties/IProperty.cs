using System;
using Common.Defs;
using Unity.Entities;

namespace Game.Model.Properties
{
    /// <summary>
    /// Свойства объекта (Health, shield, armor и т.п.)
    /// </summary>
    public interface IProperty : IDefineable, IComponentData
    {
        float Value { get; }
        float Normalize { get; }
    }
}