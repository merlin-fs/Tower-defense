using System;
using UnityEngine;

namespace Game
{
    [Serializable]
    public struct ObjectTypeID
    {
        [SerializeField]
        private string m_ID;

        public ObjectTypeID(string id)
        {
            m_ID = id;
        }

        public static implicit operator string(ObjectTypeID value) => value.m_ID;
        public static implicit operator ObjectTypeID(string value) => new ObjectTypeID(value);

        public static bool operator ==(ObjectTypeID left, ObjectTypeID right) => left.m_ID == right.m_ID;
        public static bool operator !=(ObjectTypeID left, ObjectTypeID right) => left.m_ID != right.m_ID;
        public override string ToString() => m_ID;
        public bool Equals(ObjectTypeID other) => m_ID == other.m_ID;
        public override bool Equals(object obj) => obj is ObjectTypeID id && Equals(id);
        public override int GetHashCode() => m_ID?.GetHashCode() ?? 0;

        public bool IsEmpty => string.IsNullOrEmpty(m_ID);
        public static ObjectTypeID Empty => "";
    }
}

