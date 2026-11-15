using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using InGame.Monsters;
using InGame.System;
using UnityEngine;

namespace InGame.Managers
{
    public class BloodLuckMonsterManager : IBloodLuckContext, IBloodLuckPriorityUpdatable
    {
        private BloodLuckWaveManager _bloodLuckWaveManager;
        private readonly List<BloodLuckMonster> _monsters = new();
        private readonly BloodLuckAssetHolder _assetHolder;
        private readonly BloodLuckInGameParameter _inGameParameter;
        private BloodLuckEffectManager _effectManager;
        private BloodLuckPlayerStateManager _playerStateManager;
        private BloodLuckMonsterProjectileManager _monsterProjectileManager;
        private PlayerMovement _player;

        public BloodLuckMonsterManager(BloodLuckAssetHolder assetHolder, BloodLuckInGameParameter inGameParameter)
        {
            _assetHolder = assetHolder;
            _inGameParameter = inGameParameter;
        }
        public UniTask InitAsync()
        {
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
            _bloodLuckWaveManager = contextMap.GetRegisteredInstance<BloodLuckWaveManager>();
            var updateLoopManager = contextMap.GetRegisteredInstance<BloodLuckGameUpdateLoopManager>();
            updateLoopManager.RegisterUpdatable(this);
            _effectManager = contextMap.GetRegisteredInstance<BloodLuckEffectManager>();
            _playerStateManager = contextMap.GetRegisteredInstance<BloodLuckPlayerStateManager>();
            _monsterProjectileManager = contextMap.GetRegisteredInstance<BloodLuckMonsterProjectileManager>();

            _player = Object.FindAnyObjectByType<PlayerMovement>();
        }

        public BloodLuckUpdatePriority UpdatePriority => BloodLuckUpdatePriority.MonsterUpdate;
        public void GameUpdate(float deltaTime)
        {
            foreach (var monster in _monsters)
            {
                monster.UpdateMonster(deltaTime);
            }
        }

        public void OnMonsterDied(BloodLuckMonster monster)
        {
            monster.OnMonsterDied();
            _monsters.Remove(monster);

            if (_monsters.Count == 0)
            {
                OnAllMonsterDied();
            }
        }

        private void OnAllMonsterDied()
        {
            _bloodLuckWaveManager.OnWaveFinished();
        }

        public void SpawnMonsters()
        {
            Transform target = _player.transform;
            for (int i = 0; i < _inGameParameter.MonsterSpawnCount; i++)
            {
                _monsters.Add(new BloodLuckMonster(
                    _assetHolder,
                    GetRandomSpawnPosition(),
                    GetRandomMonsterType(),
                    _inGameParameter,
                    target,
                    _playerStateManager,
                    _effectManager,
                    _monsterProjectileManager));
            }
        }
        
        private BloodLuckMonsterType GetRandomMonsterType()
        {
            return Random.value < _inGameParameter.RangedMonsterRatio
                ? BloodLuckMonsterType.Ranged
                : BloodLuckMonsterType.Melee;
        }

        private Vector3 GetRandomSpawnPosition()
        {
            Vector3 center = _player.transform.position;
            Vector2 offset = Random.insideUnitCircle * _inGameParameter.MonsterSpawnRadius;
            return center + (Vector3)offset;
        }

        public void CheatKillAllMonsters()
        {
            foreach (var monster in _monsters.ToList())
            {
                OnMonsterDied(monster);
            }
        }
    }
}