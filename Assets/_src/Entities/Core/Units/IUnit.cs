using System;
using Unity.Entities;
using Common.Defs;

namespace Game.Model.Units
{
    public interface IUnit : IDefineable, IComponentData
    {
    }
}