using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Inspector
{
    [CustomPropertyDrawer(typeof(SceneSelectorFieldAttribute))]
    public class SceneSelectorFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
                return;

            string[] scenes = EditorBuildSettings.scenes
                .Select(x => PlayFromScene.AsSpacedCamelCase(Path.GetFileNameWithoutExtension(x.path)))
                .ToArray();

            Rect popupPosition = GetPopupPosition(position);
            int currentIndex = Array.IndexOf(scenes, property.stringValue);
            int selectedIndex = EditorGUI.Popup(popupPosition, currentIndex, scenes);
            if (selectedIndex >= 0 && selectedIndex < scenes.Length)
            {
                property.stringValue = scenes[selectedIndex];
            }

            EditorGUI.PropertyField(position, property, label, true);
        }

        Rect GetPopupPosition(Rect currentPosition)
        {
            Rect popupPosition = new Rect(currentPosition);
            popupPosition.width -= EditorGUIUtility.labelWidth;
            popupPosition.x += EditorGUIUtility.labelWidth;
            popupPosition.height = EditorGUIUtility.singleLineHeight;
            return popupPosition;
        }

    }
}
        