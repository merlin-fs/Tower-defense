using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Core
{
    public interface ISkill: ISlice
    {
        new ISkill Clone();
    }
}