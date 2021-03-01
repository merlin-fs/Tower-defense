using UnityEngine;

namespace TowerDefense.Core.View
{
    public interface ISliceVisualizer
    {
        void UpdateView(IUnit unit, ISlice slice, float deltaTime);
    }


    public interface ISliceVisualizer<I> : ISliceVisualizer where I : ISlice
    {
    }
}