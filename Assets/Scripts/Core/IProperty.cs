using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Core
{
    public interface IProperty : ISlice
    {
        float Value { get; }
        new IProperty Clone();
    }
}