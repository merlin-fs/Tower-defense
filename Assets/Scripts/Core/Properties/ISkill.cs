using System.Collections.Generic;

namespace TowerDefense.Core
{
    public interface ISkill : ISlice, ISliceInit, ISliceUpdate
    {
        /// <summary>
        /// Эфекты которые будут накладываться на unit.Target
        /// </summary>
        List<IInfluence> Effects { get; }
    }
}