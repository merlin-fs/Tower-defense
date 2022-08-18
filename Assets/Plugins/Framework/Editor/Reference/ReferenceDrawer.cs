using System;
using System.IO;
using System.Reflection;
using System.Reflection.Ext;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace UnityEditor.Inspector
{
    [CustomPropertyDrawer(typeof(ReferenceAttribute))]
    class ReferenceDrawer : BaseReferenceDrawer
    {
        GUIContent m_Empty = new GUIContent("");

        protected override void GetDisplayValue(object value, ref string display)
        {
            display = display.Replace("Def", "");
        }

        protected override void OnSelect(SerializedProperty property, Type type)
        {
            var value = (type != null) ? Activator.CreateInstance(type) : null;
            property.managedReferenceValue = value ?? null;
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
        }

        protected override Type GetBaseType(SerializedProperty property)
        {
            ReferenceAttribute attr = (ReferenceAttribute)attribute;
            Type fieldType = TypeHelper.GetRealTypeFromTypename(property.managedReferenceFieldTypename);
            return attr.FieldType ?? fieldType;
        }

        protected override void FinalizeProperty(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, m_Empty, true);
        }

        protected override Rect GetRect(Rect position, SerializedProperty property, GUIContent label)
        {
            position = base.GetRect(position, property, label);
            position.height = EditorGUIUtility.singleLineHeight;
            position.x += 5;
            position.width -= 5;
            return position;
        }
    }
}
