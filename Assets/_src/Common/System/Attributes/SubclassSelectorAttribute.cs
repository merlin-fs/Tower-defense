using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class SubclassSelectorAttribute : PropertyAttribute
{
    public SubclassSelectorAttribute(Type type, bool unique = false)
    {
        FieldType = type;
        Unique = unique;
    }

    public Type FieldType { get; }
    public bool Unique { get; }
}
