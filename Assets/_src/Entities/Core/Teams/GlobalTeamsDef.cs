using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Common.Defs;

namespace Game.Model.Core
{
    [Serializable]
    public struct TeamValue
    {
        public uint Value;

        private TeamValue(uint value) => Value = value;

        public static implicit operator uint(TeamValue value) => value.Value;
        public static implicit operator TeamValue(uint value) => new TeamValue(value);
    }


    [CreateAssetMenu(fileName = "Teams", menuName = "Defs/Teams")]
    public class GlobalTeamsDef : ScriptableDef
    {
        [SerializeField]
        private string[] m_Teams;

        public string[] Teams => m_Teams;


        public int GetIndex(TeamValue value)
        {
            return (int)Mathf.Log(value.Value, 2);
        }

        public TeamValue GetTeam(string value)
        {
            var idx = Array.IndexOf(m_Teams, value);
            return idx == -1 
                ? 0 
                : GetTeam(idx);
        }

        public TeamValue GetTeam(int value)
        {
            return (uint)Math.Pow(2, value);
        }

        #region Def
        protected override void AddComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            throw new NotImplementedException();
        }
        protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            throw new NotImplementedException();
        }
        protected override void RemoveComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}