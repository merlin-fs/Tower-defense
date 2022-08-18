using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Common.Defs;

using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using TMPro;

using Game.Core;
using Game.Core.Repositories;
using Game.Model.Units.Defs;
using Game.Model.Units;
using Game.Model.World;

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

    Map m_Map;

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
        m_Pack.onClick.AddListener(() => OnSpawn(1000));
        m_GenerateMapButton.onClick.AddListener(() => GenerateMap(null));

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
        HealthDef.Initialize(m_CanvasParent, m_Prefab);
        
        LogicDef.Initialize();


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
                m_Map = map;
               // CreatePlayerBuild();
            });
    }

    private void GenerateMap(Action<Map> callback)
    {
        m_GenerateMap.Execute(callback);
    }
    

    private async void CreatePlayerBuild()
    {
        var def = (await m_Build.LoadAssetAsync().Task) as IUnitDef;
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        SpawnObject(m_Map, def, m_PlayerTeam, manager);
    }

    private void OnSpawn(int count)
    {
        StartCoroutine(SpawnObjects(count));
    }

    private void SpawnObject(Map map, IUnitDef def, ITeamDef team, EntityManager manager)
    {
        var entity = manager.CreateEntity(typeof(TestSpawn.SpawnState), typeof(Teams), typeof(HealthView));
        team.AddComponentData(entity, manager, null);
        manager.SetComponentData(entity, new TestSpawn.SpawnState() { Prefab = def.EntityPrefab });
        //var position = manager.GetComponentData<SetPositionOnMap>(def.EntityPrefab);
        //var position = new SetPositionOnMap();

        //GeneratePosition(map, ref position);
        //manager.AddComponentData(entity, position);
    }

    unsafe void GeneratePosition(Map map, ref SetPositionOnMap position)
    {
        using (var cells = map.GetCells(position.InitPosition, 5,
            (m, value) =>
            {
                bool result = m.Tiles.EntityExist(value);
                result |= IsNotPassable(map.Tiles.HeightTypes[map.At(value)].Value);

                return !result;
            }))
        {
            position.InitPosition = cells.RandomElement();
        }

        using (var cells = map.GetCells(position.TargetPosition, 5,
            (m, value) =>
            {
                //m.GetCostTile
                bool result = m.Tiles.EntityExist(value);
                result |= IsNotPassable(m.Tiles.HeightTypes[map.At(value)].Value);

                return !result;
            }))
        {
            position.TargetPosition = cells.RandomElement();
        }

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


    IEnumerator SpawnObjects(int count)
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        for (int i = 0; i < count; i++)
        {
            var rnd = UnityEngine.Random.Range(0, m_Defs.Length);
            var def = m_Defs[rnd].Asset as IUnitDef;
            SpawnObject(m_Map, def, m_Team, manager);
            yield return null;// new WaitForSeconds(0.1f);
        }
        yield break;
    }

    private void Update()
    {
        m_Text.text = $"Entities: {World.DefaultGameObjectInjectionWorld.EntityManager.Debug.EntityCount}";
    }
}
