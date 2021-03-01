//using Sirenix.OdinInspector;
using TowerDefense.Core;
using UnityEngine;


namespace TowerDefense
{
    public class UnitEnemy : Unit
    {
        public float DistFromDestination { get; set; }
        [System.NonSerialized]
        public SubWave SubWave;
        //[SerializeField, ShowInInspector]
        private Unit m_Target;
        public Unit Target { get => m_Target; private set => m_Target = value; }
    }
}