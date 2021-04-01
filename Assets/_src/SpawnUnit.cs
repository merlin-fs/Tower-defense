using System.Collections.Generic;
using UnityEngine;

namespace Game.Entities
{

    public class SpawnUnit : MonoBehaviour
    {
        [SerializeField]
        private UnitContainer m_Unit;

        [SerializeReference, SubclassSelector(typeof(IProperty))]
        private List<IProperty> m_Properties = new List<IProperty>();

        [SerializeReference, SubclassSelector(typeof(ISkill))]
        private List<ISkill> m_Skills = new List<ISkill>();

        public IReadOnlyCollection<IProperty> Properties => m_Properties;
        public IReadOnlyCollection<ISkill> Skills => m_Skills;

        public IUnit Spawn(GameObject parent)
        {
            IUnit unit = m_Unit.Value.Instantiate<IUnit>();

            unit.GameObject.transform.parent = parent.transform;
            unit.GameObject.SetActive(false);

            foreach (var iter in Properties)
                unit.AddProperty(iter.Instantiate<IProperty>());

            foreach (var iter in Skills)
                unit.AddSkill(iter.Instantiate<ISkill>());

            return unit;
        }
    }
}