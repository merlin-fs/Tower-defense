using System;
using UnityEngine;

namespace Common.Singletons
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class SingletonAttribute : Attribute
    {
        public SingletonAttribute()
        {
            HideFlags = HideFlags.None;
        }
        public bool Persistent { get; set; } = true;
        public HideFlags HideFlags { get; set; }
        public bool Automatic { get; set; } = true;
        public string Name { get; set; }
    }
}
