using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Core
{
    public interface ISlice
    {
        void Init(Unit unit);
        void Update(Unit unit, float deltaTime);
        ISlice Clone();
    }

    public class Slice: ISlice
    {
        public void Init(Unit unit) { }
        public void Update(Unit unit, float deltaTime) { }
        public ISlice Clone()
        {
            return new Slice();
        }
    }
}