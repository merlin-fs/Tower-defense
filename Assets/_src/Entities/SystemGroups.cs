using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Game
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class GameSpawnSystemGroup : ComponentSystemGroup { }
    
    [UpdateInGroup(typeof(GameSpawnSystemGroup), OrderFirst = true)]
    public class GameSpawnSystemCommandBufferSystem : BeginInitializationEntityCommandBufferSystem { }


    [UpdateAfter(typeof(TransformSystemGroup))]
    public class GameTransformSystemGroup: ComponentSystemGroup { }

    //[UpdateInGroup(typeof(GameTransformSystemGroup), OrderLast = true)]
    //public class GameTransformCommandBufferSystem : BeginVariableRateSimulationEntityCommandBufferSystem { }
    //public class GameTransformCommandBufferSystem : EndSimulationEntityCommandBufferSystem { }


    [UpdateAfter(typeof(GameTransformSystemGroup))]
    public class GameLogicInitSystemGroup : ComponentSystemGroup { }

    //[UpdateInGroup(typeof(GameWeaponInitSystemGroup), OrderFirst = true)]
    //public class GameWeaponInitCommandBufferSystem : BeginSimulationEntityCommandBufferSystem { }
    //public class GameWeaponInitCommandBufferSystem : EndSimulationEntityCommandBufferSystem { }


    [UpdateAfter(typeof(GameLogicInitSystemGroup))]
    public class GameLogicSystemGroup : ComponentSystemGroup { }

    //[UpdateInGroup(typeof(GameWeaponWorkSystemGroup), OrderLast = true)]
    //public class GameWeaponWorkCommandBufferSystem : BeginSimulationEntityCommandBufferSystem { }
    //public class GameWeaponWorkCommandBufferSystem : EndSimulationEntityCommandBufferSystem { }

    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    public class GameLogicCommandBufferSystem : BeginInitializationEntityCommandBufferSystem { }

    [UpdateAfter(typeof(GameLogicSystemGroup))]
    public class GameLogicDoneSystemGroup : ComponentSystemGroup { }


    [UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
    public class GameDoneSystemGroup : ComponentSystemGroup { }
    
    [UpdateInGroup(typeof(GameDoneSystemGroup), OrderLast = true)]
    public class GameDoneSystemCommandBufferSystem : EndSimulationEntityCommandBufferSystem { }


    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class GamePresentationSystemGroup : ComponentSystemGroup { }

}
