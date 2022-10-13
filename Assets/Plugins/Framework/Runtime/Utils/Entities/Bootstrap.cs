using System;
using Unity.Entities;

namespace Common.Entities.Tools
{
    public static class Bootstrap
    {
        public static T AddInitSystem<T>() where T : ComponentSystemBase
        {
            return AddSystemToGroup<T, InitializationSystemGroup>();
        }
        public static T AddSimSystem<T>() where T : ComponentSystemBase
        {
            return AddSystemToGroup<T, SimulationSystemGroup>();
        }
        public static T AddLateSimSystem<T>() where T: ComponentSystemBase
        {
            return AddSystemToGroup<T, LateSimulationSystemGroup>();
        }
        public static T AddRenderSystem<T>() where T : ComponentSystemBase
        {
            return AddSystemToGroup<T,PresentationSystemGroup>();
        }
        public static TSystem AddSystemToGroup<TSystem,TGroup>()
            where TSystem : ComponentSystemBase
            where TGroup : ComponentSystemGroup
        {
            var group = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<TGroup>();
            var system = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<TSystem>();
            group.AddSystemToUpdateList(system);
            return system;
        }

    }
}