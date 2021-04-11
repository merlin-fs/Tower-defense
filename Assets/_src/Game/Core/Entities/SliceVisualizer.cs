using System;
using St.Common.Core;

namespace Game.Entities.View
{
    public interface ISliceVisualizer: ISlice, ISliceInit
    {
        void UpdateView(IUnit unit, ISlice slice, float deltaTime);
    }


    public interface ISliceVisualizer<I> : ISliceVisualizer where I : ISlice
    {
    }
}