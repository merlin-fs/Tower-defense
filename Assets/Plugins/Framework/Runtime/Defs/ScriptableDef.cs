using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using System.Threading;

namespace Common.Defs
{
    using Core;

    [Serializable]
    public struct DefineableType
    {
        [SerializeField]
        private string m_Type;
        private Func<Type> m_BaseType;
        private Func<Type> m_OwnerType;
        public Type Type => string.IsNullOrEmpty(m_Type) 
            ? null 
            : Type.GetType(m_Type);

        public Type BaseType => m_BaseType();
        public Type OwnerType => m_OwnerType();

        public void SetType(Type type)
        {
            m_Type = type?.AssemblyQualifiedName ?? "";
        }
        public void SetBaseType(Func<Type> type)
        {
            m_BaseType = type;
        }
        public void SetOwnerType(Func<Type> type)
        {
            m_OwnerType = type;
        }

        public DefineableType(Func<Type> baseType, Func<Type> owner, Type type)
        {
            m_BaseType = baseType;
            m_OwnerType = owner;
            m_Type = type?.AssemblyQualifiedName;
        }
    }

    public static class DefExtensions
    {
        public static void AddComponentIData<T>(this EntityManager manager, Entity entity, T componentData)
            where T : IComponentData
        {
            Type DefType = componentData.GetType();

            if (!m_Infos.TryGetValue(DefType, out DefineableInfo value) || value.ManagerAdd == null)
            {
                var type = manager.GetType();
                if (value == null)
                    value = new DefineableInfo();
                value.ManagerAdd = type.GetMethods()
                    .First(m => m.Name == "AddComponentData" && m.ReturnParameter.ParameterType == typeof(bool));
                value.ManagerAdd = value.ManagerAdd.MakeGenericMethod(DefType);
                m_Infos[DefType] = value;
            }
            value.ManagerAdd.Invoke(manager, new object[] { entity, componentData });
        }

        public static void AddComponentIData<T>(this EntityCommandBuffer.ParallelWriter manager, Entity entity, T componentData, int sortKey)
            where T : IComponentData
        {
            Type DefType = componentData.GetType();

            if (!m_Infos.TryGetValue(DefType, out DefineableInfo value) || value.WriterAdd == null)
            {
                var type = manager.GetType();
                if (value == null)
                    value = new DefineableInfo();
                value.WriterAdd = type.GetMethods()
                    .First(m => m.Name == "AddComponent" && m.GetParameters().Length == 3 && m.GetParameters()[1].ParameterType == typeof(Entity));

                value.WriterAdd = value.WriterAdd.MakeGenericMethod(DefType);
                m_Infos[DefType] = value;
            }
            value.WriterAdd.Invoke(manager, new object[] { sortKey, entity, componentData });
        }

        protected class DefineableInfo
        {
            public MethodInfo ManagerAdd;
            public MethodInfo WriterAdd;
        }
        private static Dictionary<Type, DefineableInfo> m_Infos = new Dictionary<Type, DefineableInfo>();
    }


    public abstract class ScriptableDef : ScriptableObject, IDef
    {
        #region IDef
        void IDef.AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            AddComponentData(entity, manager, conversionSystem);
        }
        void IDef.AddComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            AddComponentData(entity, writer, sortKey);
        }

        void IDef.RemoveComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            RemoveComponentData(entity, writer, sortKey);
        }

        protected abstract void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem);
        protected abstract void AddComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey);
        protected abstract void RemoveComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey);
        #endregion
    }

    public abstract class ScriptableDef<T> : ScriptableDef, IDef<T>, ISerializationCallbackReceiver
        where T : IDefineable, IComponentData
    {
        [SerializeField]
        protected DefineableType m_DefineableType;

        private GCHandle m_SelfLinkHandle;

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_DefineableType.SetBaseType(() => typeof(T));
            m_DefineableType.SetOwnerType(() => GetType());
        }

        private string m_NameID = null;
        public string NameID => GetNameID;
        protected virtual string GetNameID
        {
            get {
                if (string.IsNullOrEmpty(m_NameID))
                    UnityMainThread.Context.Send(o => m_NameID = (o as ScriptableObject) ? (o as ScriptableObject).name : null, this);
                return m_NameID;
            }
        }
        #region IDef
        protected T CreateInstance()
        {
            Type targetType = m_DefineableType.Type;
            if (targetType == null)
                targetType = GetTargetType();

            if (targetType == null)
            {
                throw new NullReferenceException("Target type of def " + GetType().Name + " has a null target type");
            }

            //T value = (T)Activator.CreateInstance(targetType);
            T value = InitDefineable(targetType);
            return value;
        }

        public T InitDefineable(Type targetType)
        {
            T value = default;
            if (!m_Initializes.TryGetValue(targetType, out DefInfo info))
            {
                var def = targetType.FindInterfaces(
                    (t, o) =>
                    {
                        return (t.GetInterface(nameof(IDefineable)) != null && t.IsGenericType);

                    }, null);

                if (def.Length > 0)
                {
                    Type gType = def[0].GetGenericArguments()[0];
                    info.ReferenceType = typeof(ReferenceObject<>).MakeGenericType(gType);
                    info.Initialize = targetType.GetConstructor(new Type[] { info.ReferenceType });
                }
                else
                    info = new DefInfo() { Initialize = targetType.GetConstructor(new Type[] { }), ReferenceType = null };

                m_Initializes.Add(targetType, info);
            }

            if (info.Initialize == null)
                throw new MissingMethodException($"{targetType} constructor with parameter {info.ReferenceType} not found!");

            value = info.ReferenceType != null
                ? (T)info.Initialize.Invoke(new object[] { Activator.CreateInstance(info.ReferenceType, m_SelfLinkHandle) })
                : (T)info.Initialize.Invoke(new object[] { });
            return value;
        }

        protected Type GetTargetType()
        {
            Type findType = GetType();
            if (m_Targets.TryGetValue(findType, out Type value))
                return value;
            DefineableAttribute attr = findType.GetCustomAttribute<DefineableAttribute>(true);
            value = attr?.InstanceType;
            if (value != null)
                return value;
            m_Targets.Add(findType, value);
            return value;
        }

        private struct DefInfo
        {
            public ConstructorInfo Initialize;
            public Type ReferenceType;
        }

        private static Dictionary<Type, DefInfo> m_Initializes = new Dictionary<Type, DefInfo>();

        protected static Dictionary<Type, Type> m_Targets = new Dictionary<Type, Type>();

        #endregion
        public ScriptableDef()
        {
            m_SelfLinkHandle = GCHandle.Alloc(this);
            m_DefineableType = new DefineableType(() => typeof(T), () => GetType(), GetTargetType());
        }

        private void OnDestroy()
        {
            m_SelfLinkHandle.Free();
        }

        #region entity added
        protected virtual void InitializeDataConvert(ref T value, Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem) { }

        protected virtual void InitializeDataRuntime(ref T value) { }

        protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            T data = CreateInstance();
            InitializeDataConvert(ref data, entity, manager, conversionSystem);
            manager.AddComponentIData(entity, data);
        }
        protected override void AddComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            T data = CreateInstance();
            InitializeDataRuntime(ref data);
            writer.AddComponentIData(entity, data, sortKey);
        }

        protected override void RemoveComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            writer.RemoveComponent<T>(sortKey, entity);
        }
        #endregion
    }


    [Serializable]
    public abstract class ClassDef : IDef
    {
        #region IDef
        void IDef.AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            AddComponentData(entity, manager, conversionSystem);
        }
        void IDef.AddComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            AddComponentData(entity, writer, sortKey);
        }

        void IDef.RemoveComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            RemoveComponentData(entity, writer, sortKey);
        }

        protected abstract void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem);
        protected abstract void AddComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey);
        protected abstract void RemoveComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey);
        #endregion
    }

    [Serializable]
    public abstract class ClassDef<T> : ClassDef, IDef<T>, ISerializationCallbackReceiver
        where T : IDefineable, IComponentData
    {
        [SerializeField]
        protected DefineableType m_DefineableType;

        private GCHandle m_SelfLinkHandle;

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_DefineableType.SetBaseType(() => typeof(T));
            m_DefineableType.SetOwnerType(() => GetType());
        }
        #region IDef
        protected T CreateInstance()
        {
            Type targetType = m_DefineableType.Type;
            if (targetType == null)
                targetType = GetTargetType();

            if (targetType == null)
            {
                throw new NullReferenceException("Target type of def " + GetType().Name + " has a null target type");
            }

            //T value = (T)Activator.CreateInstance(targetType);
            T value = InitDefineable(targetType);
            return value;
        }

        public T InitDefineable(Type targetType)
        {
            T value = default;
            if (!m_Initializes.TryGetValue(targetType, out DefInfo info))
            {
                var def = targetType.FindInterfaces(
                    (t, o) =>
                    {
                        return (t.GetInterface(nameof(IDefineable)) != null && t.IsGenericType);

                    }, null);

                if (def.Length > 0)
                {
                    Type gType = def[0].GetGenericArguments()[0];
                    info.ReferenceType = typeof(ReferenceObject<>).MakeGenericType(gType);
                    info.Initialize = targetType.GetConstructor(new Type[] { info.ReferenceType });
                }
                else
                    info = new DefInfo() { Initialize = targetType.GetConstructor(new Type[] { }), ReferenceType = null };

                m_Initializes.Add(targetType, info);
            }

            if (info.Initialize == null)
                throw new MissingMethodException($"{targetType} constructor with parameter {info.ReferenceType} not found!");

            value = info.ReferenceType != null
                ? (T)info.Initialize.Invoke(new object[] { Activator.CreateInstance(info.ReferenceType, m_SelfLinkHandle) })
                : (T)info.Initialize.Invoke(new object[] { });
            return value;
        }

        protected Type GetTargetType()
        {
            Type findType = GetType();
            if (m_Targets.TryGetValue(findType, out Type value))
                return value;
            DefineableAttribute attr = findType.GetCustomAttribute<DefineableAttribute>(true);
            value = attr?.InstanceType;
            if (value != null)
                return value;
            m_Targets.Add(findType, value);
            return value;
        }

        private struct DefInfo
        {
            public ConstructorInfo Initialize;
            public Type ReferenceType;
        }

        private static Dictionary<Type, DefInfo> m_Initializes = new Dictionary<Type, DefInfo>();

        protected static Dictionary<Type, Type> m_Targets = new Dictionary<Type, Type>();

        #endregion
        public ClassDef()
        {
            m_SelfLinkHandle = GCHandle.Alloc(this);
            m_DefineableType = new DefineableType(() => typeof(T), () => GetType(), GetTargetType());
        }

        ~ClassDef()
        {
            m_SelfLinkHandle.Free();
        }

        #region entity added
        protected virtual void InitializeDataConvert(ref T value, Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
        }

        protected virtual void InitializeDataRuntime(ref T value)
        {
        }

        protected override void AddComponentData(Entity entity, EntityManager manager, GameObjectConversionSystem conversionSystem)
        {
            T data = CreateInstance();
            InitializeDataConvert(ref data, entity, manager, conversionSystem);
            manager.AddComponentIData(entity, data);
        }
        protected override void AddComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            T data = CreateInstance();
            InitializeDataRuntime(ref data);
            writer.AddComponentIData(entity, data, sortKey);
        }

        protected override void RemoveComponentData(Entity entity, EntityCommandBuffer.ParallelWriter writer, int sortKey)
        {
            writer.RemoveComponent<T>(sortKey, entity);
        }
        #endregion
    }
}