using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace System.Inspector
{
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        private bool m_InitializeFold = false;
        private List<Type> m_ReflectionType;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference) 
                return;

            SubclassSelectorAttribute utility = (SubclassSelectorAttribute)attribute;
            LazyGetAllInheritedType(utility.FieldType);

            Rect popupPosition = GetPopupPosition(position);
            string[] typePopupNameArray = m_ReflectionType.Select(type => type == null ? "<null>" : type.ToString()).ToArray();
            string[] typeFullNameArray = m_ReflectionType.Select(type => type == null ? "" : string.Format("{0} {1}", type.Assembly.ToString().Split(',')[0], type.FullName)).ToArray();

            //Get the type of serialized object 
            int currentTypeIndex = Array.IndexOf(typeFullNameArray, property.managedReferenceFullTypename);
            Type currentObjectType = m_ReflectionType[currentTypeIndex];
            int selectedTypeIndex = EditorGUI.Popup(popupPosition, currentTypeIndex, typePopupNameArray);
            if (selectedTypeIndex >= 0 && selectedTypeIndex < m_ReflectionType.Count)
            {
                if (currentObjectType != m_ReflectionType[selectedTypeIndex])
                {
                    property.managedReferenceValue = m_ReflectionType[selectedTypeIndex] == null 
                        ? null 
                        : Activator.CreateInstance(m_ReflectionType[selectedTypeIndex]);
                    currentObjectType = m_ReflectionType[selectedTypeIndex];
                }
            }

            if (m_InitializeFold == false)
            {
                property.isExpanded = false;
                m_InitializeFold = true;
            }
            EditorGUI.PropertyField(position, property, label, true);
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }


        void LazyGetAllInheritedType(Type baseType)
        {
            if (m_ReflectionType != null) 
                return;

            m_ReflectionType = baseType.Assembly.GetTypes()
                .Where(x => !x.IsAbstract)
                .Where(x => !x.IsGenericTypeDefinition)
                .Where(x => baseType.IsAssignableFrom(x))
                .ToList();

            m_ReflectionType.Insert(0, null);
            /*
            m_ReflectionType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => baseType.IsAssignableFrom(p) && p.IsClass)
                .ToList();
            m_ReflectionType.Insert(0, null);
            */


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