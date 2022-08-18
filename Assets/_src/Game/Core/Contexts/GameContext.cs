using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common.Core;

namespace Game.Core
{
    using Model.Units.Defs;

    public class GameContext : DIContext
    {
        [SerializeField]
        GlobalTeamsDef m_GlobalTeams;

        [SerializeField]
        Canvas m_UnitUICanvas;

        protected override void OnBind()
        {
            Bind(m_GlobalTeams);
            Bind(m_UnitUICanvas, "unit");
        }
    }
}
