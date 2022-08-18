using System;
using System.Reflection.Ext;
using UnityEngine;

namespace UnityEditor.Inspector
{
    [CustomPropertyDrawer(typeof(DefineableSelectAttribute))]
    class SelectTypeDrawer : BaseReferenceDrawer
    {
        protected override void GetDisplayValue(object value, ref string display)
        {
            display = (string)value ?? "(null)";
        }

        protected override void OnSelect(SerializedProperty property, Type type)
        {
            string value = type?.Name;
            property.stringValue = value ?? null;
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
        }

        protected override Type GetBaseType(SerializedProperty property)
        {
            DefineableSelectAttribute attr = (DefineableSelectAttribute)attribute;
            var type = attr.InstanceType;
            var method = type.GetMethod("GetGenericType");
            type = (Type)method.Invoke(null, null);
            return type;
        }

        protected override Rect GetRect(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            return EditorGUI.PrefixLabel(position, label); 
        }
    }
}
