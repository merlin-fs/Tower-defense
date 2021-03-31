using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common.Core;


namespace TowerDefense.Units
{
    using Core;

    public class SpawnUnitStatic : MonoBehaviour
    {
        [SerializeReference, SubclassSelector(typeof(IProperty))]
        private List<IProperty> m_Properties = new List<IProperty>();

        [SerializeReference, SubclassSelector(typeof(ISkill))]
        private List<ISkill> m_Skills = new List<ISkill>();

        public IReadOnlyCollection<IProperty> Properties => m_Properties;
        public IReadOnlyCollection<ISkill> Skills => m_Skills;


        private void Start()
        {
            IUnit unit = GetComponent<IUnit>();

            foreach (var iter in Properties)
                unit.AddProperty(iter.Instantiate<IProperty>());
            foreach (var iter in Skills)
                unit.AddSkill(iter.Instantiate<ISkill>());

            unit.Init();

            unit.GameObject.SetActive(true);
        }
    }
}