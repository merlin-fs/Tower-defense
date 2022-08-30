using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Common.Defs;

using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using TMPro;

using Game;
using Game.Core;
using Game.Core.Repositories;
using Game.Model.Skills;
using Game.Model.Core;
using Game.Model.Units;
using Game.Model.World;
using Game.Model;

public class TestSpawn : MonoBehaviour
{
    public struct SpawnState : ISystemStateComponentData
    {
        public Entity Prefab;
    }
}


public class PrefabDef : MonoBehaviour, IConvertGameObjectToEntity
{
    public IDef Def;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        Def.AddComponentData(entity, dstManager, conversionSystem);
    }

    public void Remove()
    {
        DestroyImmediate(this, true);
    }
}


namespace Game.Model.World
{
    public partial class Map
    {
        public static Data Singleton
        {
            get {
                return Generate.Instance.HasSingleton<Data>()
                    ? Generate.Instance.GetSingleton<Data>()
                    : default;
            }
        }

        public static void SetSingleton(Data value)
        {
            Generate.Instance.SetSingleton(value);
        }

        public static unsafe bool GeneratePosition(Map.Data map, ref int2 position)
        {
            bool result = true;
            if (map.Tiles.EntityExist(position, null))
            {
                using (var cells = Map.GetCells(position, 5,
                    (value) =>
                    {
                        bool pass = map.Tiles.EntityExist(value);
                        pass |= IsNotPassable(map.Tiles.HeightTypes[map.At(value)].Value);

                        return !pass;
                    }, Unity.Collections.Allocator.Temp))
                {
                    result = cells.Count() > 0;
                    if (result)
                        position = cells.RandomElement();
                };
            }
            return result;

            static bool IsNotPassable(Map.HeightType value)
            {
                switch (value.Value)
                {
                    case Map.HeightType.Type.Snow:
                    case Map.HeightType.Type.DeepWater:
                    case Map.HeightType.Type.ShallowWater:
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
}

[DisableAutoCreation]
class DefConversion : GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
        /*
        Entities.ForEach((PrefabDef root) =>
        {
            Entity entity = GetPrimaryEntity(root);
            //var def = root.CurrentDef;
            //def.AddComponentData(entity, DstEntityManager, this);
        });
        */
    }
}

public class MainStart : MonoBehaviour
{
    public static MainStart Instance;

    [SerializeField]
    TMP_Text m_Text;

    [SerializeField]
    private Button m_Button;
    [SerializeField]
    private Button m_Pack;
    [SerializeField]
    private Button m_GenerateMapButton;
    [SerializeField]
    private Button m_CreateSquad;
    

    [SerializeField]
    AssetReferenceT<UnitDef> m_Build;
    [SerializeReference, Reference()]
    ITeamDef m_PlayerTeam;

    [SerializeField]
    private AssetReferenceT<UnitDef>[] m_Defs;

    [SerializeReference, Reference()]
    ITeamDef m_Team;

    [Header("Health view")]
    [SerializeField]
    public Canvas m_CanvasParent;
    [SerializeField]
    public HealthComponent m_Prefab;

    [Header("Map view")]
    [SerializeField]
    GenerateMap m_GenerateMap;

    [SerializeReference, Reference()]
    Squad.ISquadDef m_Squad;
    


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
#if UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP
        DefaultWorldInitialization.Initialize("Default World", false);
#endif
    }

    private async void Awake()
    {
        m_Button.onClick.AddListener(() => OnSpawn(1));
        //m_Pack.onClick.AddListener(() => OnSpawn(1000));
        m_Pack.onClick.AddListener(() => OnSpawn(20));

        m_GenerateMapButton.onClick.AddListener(() => GenerateMap(null));

        m_CreateSquad.onClick.AddListener(() => OnSpawnSquad());


        foreach (var iter in m_Defs)
        {
            if (!iter.IsValid())
                await iter.LoadAssetAsync().Task;
        }
    }

    async void Start()
    {
        Instance = this;

        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        var DstEntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        settings.ConversionFlags |= GameObjectConversionUtility.ConversionFlags.AssignName;
        settings.FilterFlags = WorldSystemFilterFlags.HybridGameObjectConversion;
        settings = settings.WithExtraSystem<DefConversion>();

        var repo = await Repositories.Instance.RepositoryAsync<UnitDef>();

        Game.Model.Properties.HealthDef.Initialize(m_CanvasParent, m_Prefab);
        
        //TODO: перенести ! (как вариант событие (шина) на инициализацию мира)
        Game.Model.Logics.EnemyLogicDef.Initialize();
        Game.Model.Logics.TowerLogicDef.Initialize();
        Game.Model.Logics.EnemySquadDef.Initialize();
        


        foreach (var iter in repo.Find())
        {
            Debug.Log($"Defs load^ {iter.NameID}");

            var obj = iter.GetPrefab().AddComponent<PrefabDef>();
            obj.Def = iter;

            var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(obj.gameObject, settings);

            obj.Remove();

            iter.Init(entity);

            var reference = DstEntityManager.CreateEntity();
            var data = new ReferencePrefab
            {
                Prefab = entity
            };
            DstEntityManager.AddComponentData(reference, data);
        }


        GenerateMap(
            (map) =>
            {
                Map.SetSingleton(map);
                CreatePlayerBuild();
            });
    }

    private void GenerateMap(Action<Map.Data> callback)
    {
        m_GenerateMap.Execute(callback);
    }
    

    private async void CreatePlayerBuild()
    {
        var def = (await m_Build.LoadAssetAsync().Task) as IUnitDef;
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        SpawnObject(Map.Singleton, def, m_PlayerTeam, manager);
    }

    private void OnSpawn(int count)
    {
        StartCoroutine(SpawnObjects(count));
    }

    private void OnSpawnSquad()
    {
        SpawnObject(Map.Singleton, m_Squad, m_Team, World.DefaultGameObjectInjectionWorld.EntityManager);
    }

    private void SpawnObject(Map.Data map, IUnitDef def, ITeamDef team, EntityManager manager)
    {
        var entity = manager.CreateEntity(typeof(TestSpawn.SpawnState), typeof(Teams), typeof(HealthView));
        team.AddComponentData(entity, manager, null);
        manager.SetComponentData(entity, new TestSpawn.SpawnState() { Prefab = def.EntityPrefab });
        //var position = manager.GetComponentData<SetPositionOnMap>(def.EntityPrefab);
        //var position = new SetPositionOnMap();

        //GeneratePosition(map, ref position);
        //manager.AddComponentData(entity, position);
    }

    private void SpawnObject(Map.Data map, Squad.ISquadDef def, ITeamDef team, EntityManager manager)
    {
        var buff = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<GameSpawnSystemCommandBufferSystem>();
        var writer = buff.CreateCommandBuffer().AsParallelWriter();

        var entity = manager.CreateEntity();
        m_Squad.AddComponentData(entity, writer, 0);
        manager.SetName(entity, "Squad");

    }

    IEnumerator SpawnObjects(int count)
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        for (int i = 0; i < count; i++)
        {
            var rnd = UnityEngine.Random.Range(0, m_Defs.Length);
            var def = m_Defs[rnd].Asset as IUnitDef;
            SpawnObject(Map.Singleton, def, m_Team, manager);
            yield return null;// new WaitForSeconds(0.1f);
        }
        yield break;
    }

    private void Update()
    {
        m_Text.text = $"Entities: {World.DefaultGameObjectInjectionWorld.EntityManager.Debug.EntityCount}";
    }
}
