using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TowerDefense.Core 
{
    [System.Serializable]
	public class SubWave
    {
		public GameObject unit;
		public int count = 1;
		public float interval = 1;
		public float delay;
		public Waypoints path;
        [SerializeReference, SerializeReferenceButton]
        public List<IProperty> Properties = new List<IProperty>();
        [SerializeReference, SerializeReferenceButton]
        public List<ISkill> Skills = new List<ISkill>();
        public SubWave Clone()
        {
            SubWave subWave = new SubWave
            {
                unit = unit,
                count = count,
                interval = interval,
                delay = delay,
                path = path
            };
            subWave.Properties.Clear();
            foreach (var prop in Properties)
                subWave.Properties.Add(prop.Clone());
            subWave.Skills.Clear();
            foreach (var skill in Skills)
                subWave.Skills.Add(skill.Clone());
            return subWave;
		}
	}
	[System.Serializable]
	public class Wave
    {
		[HideInInspector]
        public int waveID =- 1;
		public List<SubWave> subWaveList = new List<SubWave>();
		public int lifeGain = 0;
		public int energyGain = 0;
		public int scoreGain = 100;
		public List<int> rscGainList = new List<int>();
		public int activeUnitCount = 0;	//only used in runtime
		
		[HideInInspector]
        public bool spawned = false; //flag indicating weather all unit in the wave have been spawn, only used in runtime
		[HideInInspector]
        public bool cleared = false; //flag indicating weather the wave has been cleared, only used in runtime
		public float duration = 10;						//duration until next wave
		public int subWaveSpawnedCount = 0;		//the number of subwave that finish spawning, used to check if all the spawning is done, only used in runtime
		
		//calculate the time require to spawn this wave
		public float CalculateSpawnDuration()
        {
			float duration = 0;
			for (int i = 0; i < subWaveList.Count; i++)
            {
				SubWave subWave = subWaveList[i];
				float thisDuration = ((subWave.count-1) * subWave.interval) + subWave.delay;
				if (thisDuration > duration)
                {
					duration = thisDuration;
				}
			}
			return duration;
		}
		
		public Wave Clone()
        {
            Wave wave = new Wave
            {
                duration = duration,
                scoreGain = scoreGain
            };
            foreach (var subWawe in subWaveList)
                wave.subWaveList.Add(subWawe.Clone());
            foreach (var gain in rscGainList)
                wave.rscGainList.Add(gain);
			return wave;
		}
	}
	
}
