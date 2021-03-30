using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common.Core.Loading;

namespace UnityEditor.Inspector
{
    [CustomPropertyDrawer(typeof(LoadingManager.Dependency))]
    public class LoadinDependencyDrawer : PropertyDrawer
    {
        private bool m_Folded = false;
        private const float LINE_HEIGHT = 18;
        private const float SPACING = 4;
        private readonly GUIStyle m_St = new GUIStyle(EditorStyles.label);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty valueItems = property.FindPropertyRelative("m_CommandsIndex");
            string path = property.propertyPath.Split('.')
                .Where((iter, idx) => idx < property.depth - 1)
                .Aggregate((current, next) => current + "." + next);

            SerializedProperty arrayItems = property.serializedObject.FindProperty(path);

            string[] names = BuildArray();

            float x = position.x;
            float y = position.y;
            string title = $"Dependency";
            m_St.normal.textColor = new Color(0.7f, 0.1f, 0.7f, 1);

            m_Folded = EditorGUI.Foldout(new Rect(x, y, position.width - 6, LINE_HEIGHT), m_Folded, title, true);
            
            #region Draw array
            if (valueItems.arraySize > 0 && m_Folded)
            {
                for (int i = 0; i < valueItems.arraySize; ++i)
                {
                    SerializedProperty valueItem = valueItems.GetArrayElementAtIndex(i);
                    y += LINE_HEIGHT + SPACING;

                    int currentTypeIndex = valueItem.intValue;
                    
                    int selectedTypeIndex = EditorGUI.Popup(new Rect(x, y, position.width, LINE_HEIGHT + SPACING), currentTypeIndex, names);

                    if (selectedTypeIndex >= 0 && selectedTypeIndex < names.Length)
                    {
                        valueItem.intValue = selectedTypeIndex;
                    }
                }
            }

            x += LINE_HEIGHT;
            y += LINE_HEIGHT + SPACING;
            if (GUI.Button(new Rect(x, y, 60, LINE_HEIGHT + (SPACING / 2)), "Add") && valueItems.arraySize < arrayItems.arraySize-1)
            {
                valueItems.arraySize++;
                m_Folded = true;
            }
            if (GUI.Button(new Rect((x + 60 + SPACING), y, 60, LINE_HEIGHT + (SPACING / 2)), "Del") && valueItems.arraySize > 0)
            {
                valueItems.arraySize--;
                m_Folded = true;
            }
            y += LINE_HEIGHT + SPACING;
            #endregion

            string[] BuildArray()
            {
                List<string> list = new List<string>();
                for (int i = 0; i < arrayItems.arraySize; i++)
                {
                    string name = arrayItems.GetArrayElementAtIndex(i).FindPropertyRelative("Command").managedReferenceFullTypename;
                    name = name.Remove(0, name.IndexOf(" "));
                    list.Add(name);
                }
                return list.ToArray();
            }

        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineAndSpace = LINE_HEIGHT + SPACING;
            if (!m_Folded)
            {
                return lineAndSpace * 2;
            }
            else
            {
                SerializedProperty valueItems = property.FindPropertyRelative("m_CommandsIndex");
                return (valueItems.arraySize + 2) * lineAndSpace;
            }
        }

    }
}   