using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense
{
    public class UnitEnemy : Unit
    {
        public float DistFromDestination { get; set; }
        [HideInInspector]
        public SubWave SubWave;
    }
}