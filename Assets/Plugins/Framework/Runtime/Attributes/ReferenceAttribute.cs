using UnityEngine;

namespace System
{
	public class ReferenceAttribute : PropertyAttribute
    {
        public ReferenceAttribute(Type type = null, bool readOnly = false)
        {
            FieldType = type;
            ReadOnly = readOnly;
        }

        public Type FieldType { get; }
        public bool ReadOnly { get; }
    }
}
