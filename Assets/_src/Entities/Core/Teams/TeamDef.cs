using System;
using UnityEngine;
using Unity.Entities;
using Common.Defs;

namespace Game.Model.Core
{
    public interface ITeamDef : IDef
    {
        TeamValue Team { get; }
        TeamValue EnemyTeams { get; }
    }

    [Serializable]
    [Defineable(typeof(Teams))]
    public class TeamDef : ClassDef<Teams>, ITeamDef
    {
        [SerializeField]
        private TeamValue m_Team;
        [SerializeField]
        private TeamValue[] m_EnemyTeams;
        TeamValue ITeamDef.Team => m_Team;
        TeamValue ITeamDef.EnemyTeams => GetTeams(m_EnemyTeams);

        private TeamValue GetTeams(TeamValue[] values)
        {
            uint teams = 0;
            foreach (var iter in values)
                teams |= iter;
            return teams;
        }

        protected override void InitializeDataConvert(ref Teams value, Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            base.InitializeDataConvert(ref value, entity, manager, conversionSystem);
            value.Team = m_Team;
            value.EnemyTeams = GetTeams(m_EnemyTeams);
        }

        protected override void InitializeDataRuntime(ref Teams value)
        {
            base.InitializeDataRuntime(ref value);
            value.Team = m_Team;
            value.EnemyTeams = GetTeams(m_EnemyTeams);
        }
    }
}