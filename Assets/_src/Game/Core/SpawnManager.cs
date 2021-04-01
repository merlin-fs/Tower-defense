using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    using Entities;

    public class SpawnManager : MonoBehaviour//SerializedMonoBehaviour//
    {

        public delegate void NewWaveHandler(int waveID);
        public static event NewWaveHandler OnNewWave;

        public delegate void WaveSpawnedHandler(int time);
        //!!!public static event WaveSpawnedHandler OnWaveSpawned;	

        public delegate void WaveClearedHandler(int time);
        //!!!public static event WaveClearedHandler OnWaveCleared;			

        public delegate void EnableSpawnHandler();
        public static event EnableSpawnHandler OnEnableSpawn;

        public delegate void SpawnTimerHandler(float time);
        public static event SpawnTimerHandler OnSpawnTimer;     //call to indicate timer refresh for continous spawn

        [SerializeField]
        private WaypointsContainer m_DefaultPath;

        public static int CurrentWaveID { get => instance.m_CurrentWaveID; }
        public bool IsSpawningStarted { get => (m_CurrentWaveID >= 0) ? true : false; }
        public enum SpawnMode
        {
            Continous,
            WaveCleared,
            Round
        }
        public SpawnMode spawnMode;
        public bool allowSkip = false;
        public bool autoStart = false;
        public float autoStartDelay = 5;
        public bool procedurallyGenerateWave = false;   //when checked, all wave is generate procedurally

        private int m_CurrentWaveID = -1;                 //start at -1, switch to 0 as soon as first wave start, always indicate latest spawned wave's ID
        public bool spawning = false;
        public int activeUnitCount = 0;                 //for wave-cleared mode checking
        public int totalSpawnCount = 0;                 //for creep instanceID
        public int waveClearedCount = 0; 	            //for quick checking how many wave has been cleared

        public List<Wave> waveList = new List<Wave>();  //in endless mode, this is use to store temporary wave
                                                        //public WaveGenerator waveGenerator;

        public static bool AutoStart()
        {
            return instance.autoStart;
        }
        public static float GetAutoStartDelay()
        {
            return instance.autoStartDelay;
        }
        public static SpawnManager instance;

        void Awake()
        {
            if (instance != null)
                return;
            instance = this;
        }
        // Use this for initialization
        void Start()
        {
            /*!!!
            if (procedurallyGenerateWave)
            {
				waveGenerator.CheckPathList();
				if (defaultPath != null && waveGenerator.pathList.Count == 0)
                    waveGenerator.pathList.Add(defaultPath);
				
				for (int i = 0; i < waveList.Count; i++)
                {
					waveList[i] = waveGenerator.Generate(i);
				}
			}
			
			for(int i=0; i < waveList.Count; i++)
                waveList[i].waveID = i;
			*/
            if (autoStart)
                StartCoroutine(AutoStartRoutine());
        }
        IEnumerator AutoStartRoutine()
        {
            yield return new WaitForSeconds(autoStartDelay);
            DoSpawn();
        }
        void OnEnable()
        {
            Unit.OnDestroyed += OnUnitDestroyed;
            Moving.OnDestination += OnUnitReachDestination;
        }
        void OnDisable()
        {
            Unit.OnDestroyed -= OnUnitDestroyed;
            Moving.OnDestination -= OnUnitReachDestination;
        }
        void OnUnitDestroyed(IUnit unit, float delay)
        {
            if (unit.IsDead)
                OnUnitCleared(unit);

            /* !!!
            if (!unit.IsCreep()) return;
			
			UnitCreep creep=unit.GetUnitCreep();
			OnUnitCleared(creep);
            */
            StartCoroutine(DoUnitDestroyed(unit, delay));
        }
        IEnumerator DoUnitDestroyed(IUnit unit, float duration)
        {
            yield return new WaitForSeconds(duration);
            unit.Dispose();
            //PoolManager.Inst.ReturnPoolable(unit);
        }
        void OnUnitReachDestination(IUnit unit)
        {
            //only execute if creep is dead 
            //when using path-looping the creep would be still active and wouldnt set it's dead flag to true
            if (unit.IsDead)
                OnUnitCleared(unit);
        }
        void OnUnitCleared(IUnit unit)
        {
            /* !!!
			int waveID = creep.waveID;
			
			activeUnitCount-=1;
			
			Wave wave=waveList[waveID];
			
			wave.activeUnitCount-=1;
			if(wave.spawned && wave.activeUnitCount==0){
				wave.cleared=true;
				waveClearedCount+=1;
				Debug.Log("wave"+(waveID+1)+ " is cleared");
				
				ResourceManager.GainResource(wave.rscGainList);
				GameControl.GainLife(wave.lifeGain);
				
				if (IsAllWaveCleared())
                {
					GameControl.GameWon();
				}
				else if (spawnMode == SpawnMode.Round) 
                    OnEnableSpawn?.Invoke();
			}
			
			
			if(!IsAllWaveCleared() && activeUnitCount==0 && !spawning){
				if(spawnMode==_SpawnMode.WaveCleared) SpawnWaveFinite();
			}
            */
        }
        public static void Spawn()
        {
            instance.DoSpawn();
        }
        public void DoSpawn()
        {
            /* !!!
			if (GameControl.IsGameOver())
                return;
            */
            if (IsSpawningStarted)
            {
                if (spawnMode == SpawnMode.Round)
                {
                    if (!waveList[m_CurrentWaveID].cleared)
                        return;
                }
                else if (!allowSkip)
                    return;

                SpawnWaveFinite();
                return;
            }

            if (spawnMode != SpawnMode.Continous)
                SpawnWaveFinite();
            else
                StartCoroutine(ContinousSpawnRoutine());

            //spawningStarted=true;
            //!!!GameControl.StartGame();
        }
        IEnumerator ContinousSpawnRoutine()
        {
            while (true)
            {
                /*!!!
				if (GameControl.IsGameOver())
                    yield break;
				*/
                float duration = SpawnWaveFinite();

                if (m_CurrentWaveID >= waveList.Count)
                    break;
                yield return new WaitForSeconds(duration);
            }
        }
        private float SpawnWaveFinite()
        {
            if (spawning)
                return 0;

            spawning = true;
            m_CurrentWaveID++;
            if (m_CurrentWaveID >= waveList.Count)
                return 0;
            Debug.Log("spawning wave" + (m_CurrentWaveID + 1));
            OnNewWave?.Invoke(m_CurrentWaveID + 1);

            Wave wave = waveList[m_CurrentWaveID];
            if (spawnMode == SpawnMode.Continous)
            {
                if (m_CurrentWaveID < waveList.Count - 1)
                {
                    OnSpawnTimer?.Invoke(wave.duration);
                }
            }
            for (int i = 0; i < wave.subWaveList.Count; i++)
            {
                StartCoroutine(SpawnSubWave(wave.subWaveList[i], wave));
            }
            return wave.duration;
        }
        IEnumerator SpawnSubWave(SubWave subWave, Wave parentWave)
        {
            yield return new WaitForSeconds(subWave.delay);
            IWaypoints path = subWave.path.Value != null 
                ? subWave.path.Value
                : m_DefaultPath.Value;
            int spawnCount = 0;
            while (spawnCount < subWave.count)
            {
                //GameObject obj = PoolManager.Inst.GetPoolable<IUnit>(subWave.unit.GetComponent<IUnit>()).GameObject;

                IUnit unit = subWave.unit.GetComponent<IUnit>().Instantiate<IUnit>();
                UnitEnemy enemy = unit.GameObject.GetComponent<UnitEnemy>();
                unit.GameObject.SetActive(false);

                foreach (var prop in subWave.Properties)
                    unit.AddProperty(prop.Instantiate<IProperty>());
                    
                foreach (var skill in subWave.Skills)
                    unit.AddSkill(skill.Instantiate<ISkill>());

                enemy.SubWave = subWave;
                unit.Init();

                unit.GameObject.SetActive(true);
                totalSpawnCount++;
                activeUnitCount++;
                parentWave.activeUnitCount++;
                spawnCount++;
                if (spawnCount == subWave.count)
                    break;
                yield return new WaitForSeconds(subWave.interval);
            }

            parentWave.subWaveSpawnedCount++;

            if (parentWave.subWaveSpawnedCount == parentWave.subWaveList.Count)
            {
                parentWave.spawned = true;
                spawning = false;
                Debug.Log("wave " + (parentWave.waveID + 1) + " has done spawning");
                yield return new WaitForSeconds(0.5f);
                if (m_CurrentWaveID <= waveList.Count - 2)
                {
                    //for UI to show spawn button again
                    if (spawnMode == SpawnMode.Continous && allowSkip)
                        OnEnableSpawn?.Invoke();
                    if (spawnMode == SpawnMode.WaveCleared && allowSkip)
                        OnEnableSpawn?.Invoke();
                }
            }
        }
        public static bool IsAllWaveCleared()
        {
            Debug.Log("check all wave cleared   " + instance.waveClearedCount + "   " + instance.waveList.Count);
            return (instance.waveClearedCount >= instance.waveList.Count) ? true : false;
        }
        public static int GetTotalWaveCount()
        {
            return instance.waveList.Count;
        }
    }

}