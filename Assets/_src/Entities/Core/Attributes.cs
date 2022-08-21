using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Model.Units
{
    public struct WeaponReady : IComponentData
    {
        public bool IsReady;
        public static implicit operator WeaponReady(bool value) => new WeaponReady { IsReady = value };
    }

    public struct StateShot : ISystemStateComponentData
    {
    }

    public struct StateCalcProperty : ISystemStateComponentData
    {
    }

    public struct StateInit : ISystemStateComponentData
    {
    }

    public struct StateDead : ISystemStateComponentData
    {
    }

    public struct StateShotDone : ISystemStateComponentData
    {
        public float Delay;
        public float Time;
    }

    public struct HealthView : IComponentData, IDisposable
    {
        private GCHandle m_ViewHandle;
        public HealthComponent Value => (HealthComponent)m_ViewHandle.Target;

        public HealthView(HealthComponent value)
        {
            m_ViewHandle = GCHandle.Alloc(value);
        }

        public void Dispose()
        {
            m_ViewHandle.Free();
        }
    }


    public struct ParticleSpawner : IComponentData
    {
        public Entity Value;
        public static implicit operator ParticleSpawner(Entity value) => new ParticleSpawner { Value = value };
    }

    public static class NotifyExt
    {
        /*
        public static void RecursiveChilds(this BufferTypeHandle<Notify> self, ArchetypeChunk batchInChunk, int index, Action<Entity> action)
        {
            var enums = batchInChunk.GetBufferAccessor(self)[index];
            foreach (var iter in enums)
                action.Invoke(iter.Value);
        }
        */
    }
}