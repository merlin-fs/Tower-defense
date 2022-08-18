using System;
using System.Reflection;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class DefineableAttribute : Attribute
{
    public Type InstanceType { get; }

    public DefineableAttribute(Type instanceType)
    {
        InstanceType = instanceType;
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class DefineableSelectAttribute : PropertyAttribute
{
    public Type InstanceType { get; }
    public DefineableSelectAttribute(Type instanceType = null)
    {
        InstanceType = instanceType;
    }
}