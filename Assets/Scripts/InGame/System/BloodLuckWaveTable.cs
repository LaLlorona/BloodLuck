using System;
using System.Collections.Generic;
using InGame.Monsters;
using UnityEngine;

namespace InGame.System
{
    [CreateAssetMenu(fileName = "BloodLuckWaveTable", menuName = "BloodLuck/Wave Table")]
    public class BloodLuckWaveTable : ScriptableObject
    {
        [SerializeField] private List<BloodLuckWaveData> _waves = new();

        public int WaveCount => _waves.Count;

        public BloodLuckWaveData GetWave(int waveNumber)
        {
            int index = Mathf.Clamp(waveNumber - 1, 0, _waves.Count - 1);
            return _waves[index];
        }
    }

    [Serializable]
    public class BloodLuckWaveData
    {
        [SerializeField] private List<BloodLuckWaveMonsterEntry> _monsters = new();

        public IReadOnlyList<BloodLuckWaveMonsterEntry> Monsters => _monsters;
    }

    [Serializable]
    public class BloodLuckWaveMonsterEntry
    {
        [SerializeField] private BloodLuckMonsterType _monsterType;
        [SerializeField, Min(0)] private int _count = 1;
        [SerializeField, Min(1f)] private float _maxHp = 10f;
        [SerializeField, Min(0f)] private float _baseDamage = 10f;

        public BloodLuckMonsterType MonsterType => _monsterType;
        public int Count => _count;
        public float MaxHp => _maxHp;
        public float BaseDamage => _baseDamage;
    }
}
