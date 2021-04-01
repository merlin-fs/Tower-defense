using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Inspector
{
    using Game.Core;

    [CustomPropertyDrawer(typeof(Waypoints.PointObject))]
    public class WaypointsDrawer : PropertyDrawer
    {
        private const float LINE_HEIGHT = 18;
        private const float SPACING = 4;
        private const string BTN_UP = "▲";
        private const string BTN_DOWN = "▼";
        private bool _folded = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            float x = position.x;
            float y = position.y;
            float inspectorWidth = position.width;
            string[] props = new string[] { "transform", BTN_UP, BTN_DOWN };
            float[] widths = new float[] { 0.7f, 0.1f, 0.1f };
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            SerializedProperty items = property.FindPropertyRelative("Objects");
            Waypoints owner = property.serializedObject.targetObject as Waypoints;

            if (GUI.Button(new Rect(x, y, inspectorWidth, LINE_HEIGHT), "Assign using all child objects"))
            {
                List<Transform> list = new List<Transform>();
                foreach (Transform child in owner.transform)
                {
                    list.Add(child);
                }

                items.arraySize = list.Count;
                for (int i = 0; i < list.Count; i++)
                {
                    items.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
                }

                owner.Rebuild();
            }

            string title = $"Points ({items.arraySize})";
            y += LINE_HEIGHT + SPACING;
            _folded = EditorGUI.Foldout(new Rect(x, y, position.width - 6, LINE_HEIGHT), _folded, title, true);
            #region Draw array
            if (items.arraySize > 0 && _folded)
            {
                for (int i = 0; i < items.arraySize; ++i)
                {
                    SerializedProperty item = items.GetArrayElementAtIndex(i);
                    float rowX = x;
                    y += LINE_HEIGHT + SPACING;
                    for (int n = 0; n < props.Length; ++n)
                    {
                        // Calculate rects
                        float w = widths[n] * inspectorWidth;
                        Rect rect = new Rect(rowX, y, w, LINE_HEIGHT);
                        rowX += w;

                        if (n == 0)
                        {
                            EditorGUI.ObjectField(rect, item.objectReferenceValue, typeof(Transform), true);
                        }
                        else
                        {
                            if (GUI.Button(rect, props[n]))
                            {
                                switch (props[n])
                                {
                                    case BTN_DOWN:
                                        if (i > 0)
                                        {
                                            items.MoveArrayElement(i, i + 1);
                                        }
                                        break;
                                    case BTN_UP:
                                        if (i < items.arraySize - 1)
                                        {
                                            items.MoveArrayElement(i, i - 1);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            // Set indent back to what it was
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty items = property.FindPropertyRelative("Objects");
            float lineAndSpace = LINE_HEIGHT + SPACING;
            return 40 + (items.arraySize * lineAndSpace) + lineAndSpace;
        }
    }
}