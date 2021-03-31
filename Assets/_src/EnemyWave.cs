﻿using System;
using System.Collections.Generic;

using UnityEngine;

namespace TowerDefense
{
    using Core;
    using Core.View;

    [Serializable]
    public class WaypointsContainer : TypedContainer<IWaypoints> { }

    [Serializable]
    public class SubWave
    {
        public GameObject unit;
        public int count = 1;
        public float interval = 1;
        public float delay;
        public WaypointsContainer path;


        [SerializeReference, SubclassSelector(typeof(IProperty))]
        private List<IProperty> m_Properties = new List<IProperty>();

        [SerializeReference, SubclassSelector(typeof(ISkill))]
        private List<ISkill> m_Skills = new List<ISkill>();

        public IReadOnlyCollection<IProperty> Properties => m_Properties;
        public IReadOnlyCollection<ISkill> Skills => m_Skills;

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

            foreach (IProperty prop in m_Properties)
                subWave.m_Properties.Add(prop.Instantiate<IProperty>());
            foreach (ISkill skill in m_Skills)
                subWave.m_Skills.Add(skill.Instantiate<ISkill>());

            return subWave;
        }
    }
    [System.Serializable]
    public class Wave
    {
        [HideInInspector]
        public int waveID = -1;
        public List<SubWave> subWaveList = new List<SubWave>();
        public int lifeGain = 0;
        public int energyGain = 0;
        public int scoreGain = 100;
        public List<int> rscGainList = new List<int>();
        public int activeUnitCount = 0; //only used in runtime

        [HideInInspector]
        public bool spawned = false; //flag indicating weather all unit in the wave have been spawn, only used in runtime
        [HideInInspector]
        public bool cleared = false; //flag indicating weather the wave has been cleared, only used in runtime
        public float duration = 10;                     //duration until next wave
        public int subWaveSpawnedCount = 0;     //the number of subwave that finish spawning, used to check if all the spawning is done, only used in runtime

        //calculate the time require to spawn this wave
        public float CalculateSpawnDuration()
        {
            float duration = 0;
            for (int i = 0; i < subWaveList.Count; i++)
            {
                SubWave subWave = subWaveList[i];
                float thisDuration = ((subWave.count - 1) * subWave.interval) + subWave.delay;
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