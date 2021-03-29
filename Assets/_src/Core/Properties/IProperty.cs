using System;

namespace TowerDefense.Core
{
    public interface IProperty : ISlice, ISliceInit, ISliceUpdate
    {
        float Value { get; }
    }
}