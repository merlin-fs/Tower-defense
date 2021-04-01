using System.Collections.Generic;
using UnityEngine;
using St.Common.Core;

namespace Game
{
    using Entities;

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
            Restart();
        }

        private void Restart()
        {
            IUnit unit = GetComponent<IUnit>();
            unit.OnDispose += OnDeadTarget;

            foreach (var iter in Properties)
                unit.AddProperty(iter.Instantiate<IProperty>());
            foreach (var iter in Skills)
                unit.AddSkill(iter.Instantiate<ISkill>());

            unit.Init();

            unit.GameObject.SetActive(true);
        }

        private void OnDeadTarget(ICoreDisposable disposable)
        {
            IUnit unit = GetComponent<IUnit>();
            unit.Instantiate();
        }
    }
}