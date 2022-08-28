using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Ext;
using UnityEngine;
using Common.Core;
using Common.Defs;
using Game.Model.Core;

namespace UnityEditor.Inspector
{

    [CustomPropertyDrawer(typeof(TeamValue), true)]
    public class TeamDefEdit : PropertyDrawer
    {
        private GlobalTeamsDef m_GlobalTeamsDef;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var teams = NeedGlobalsTeams();
            var rect = EditorGUI.PrefixLabel(position, label);
            var names = teams.Teams;
            TeamValue value = (TeamValue)property.GetValue();

            var idx = teams.GetIndex(value);
            var newIdx = EditorGUI.Popup(rect, idx, teams.Teams);
            if (idx != newIdx)
            {
                value = teams.GetTeam(newIdx);
                property.SetValue(value);
                EditorUtility.SetDirty(property.serializedObject.targetObject);
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }
        }

        GlobalTeamsDef NeedGlobalsTeams()
        {
            if (!m_GlobalTeamsDef)
            {
                var asset = AssetDatabase.FindAssets("t:GlobalTeamsDef").First();
                m_GlobalTeamsDef = AssetDatabase.LoadAssetAtPath<GlobalTeamsDef>(AssetDatabase.GUIDToAssetPath(asset));
            }
            return m_GlobalTeamsDef;
        }

    }
}
