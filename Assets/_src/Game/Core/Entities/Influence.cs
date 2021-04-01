using System.Collections.Generic;

namespace Game.Entities
{
    public interface IInfluence : ISlice
    {
        IReadOnlyCollection<IDamage> Damages { get; }
        void Apply(IUnit sender, IUnit target);
    }
}