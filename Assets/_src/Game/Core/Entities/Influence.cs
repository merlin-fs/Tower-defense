using System.Collections.Generic;

namespace Game.Entities
{
    public interface IInfluence : ISlice, ISliceUpdate, ISliceInit
    {
        IReadOnlyCollection<IDamage> Damages { get; }
        void Activate(IUnit sender, IUnit target, float deltaTime);
    }
}