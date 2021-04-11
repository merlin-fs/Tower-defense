using System.Collections.Generic;
using UnityEngine;

namespace Game.Config
{
    using Entities;

    [CreateAssetMenu(fileName = "Unit.asset", menuName = "TowerDefense/Unit Configuration", order = 0)]
    public class UnitConfiguration : ScriptableObject
    {
        [SerializeField]
        private string m_Name;

        [SerializeField]
        private UnitContainer m_Unit;

        [SerializeReference, SubclassSelector(typeof(IProperty))]
        private List<IProperty> m_Properties = new List<IProperty>();

        [SerializeReference, SubclassSelector(typeof(ISkill))]
        private List<ISkill> m_Skills = new List<ISkill>();

        public IUnit Unit => m_Unit.Value;
        public IReadOnlyCollection<IProperty> Properties => m_Properties;
        public IReadOnlyCollection<ISkill> Skills => m_Skills;
    }
}