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

        public BloodLuckMonsterManager(BloodLuckManagerParameter parameter)
        {
            _assetHolder = parameter.AssetHolder;
            _inGameParameter = parameter.InGameParameter;
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

        // Must be called before the origin takes damage, because dead monsters lose their position.
        public void ApplyChainLightning(BloodLuckMonster origin, float damage)
        {
            if (damage <= 0f)
            {
                return;
            }

            var visited = new HashSet<BloodLuckMonster> { origin };
            var chainedMonsters = new List<BloodLuckMonster>();
            var points = new List<Vector3> { origin.Position };
            var current = origin.Position;

            for (int i = 0; i < _inGameParameter.ChainLightningMaxCount; i++)
            {
                var next = FindNearestMonster(current, visited, _inGameParameter.ChainLightningRange);
                if (next == null)
                {
                    break;
                }

                visited.Add(next);
                chainedMonsters.Add(next);
                current = next.Position;
                points.Add(current);
            }

            if (points.Count > 1)
            {
                _effectManager.ShowChainLightning(points);
            }

            foreach (var monster in chainedMonsters)
            {
                monster.OnMonsterDamage(damage, BloodLuckDamageSource.ChainLightning);
            }
        }

        public void ApplyDashDamage(Vector3 start, Vector3 end, float damage)
        {
            var hitMonsters = new List<BloodLuckMonster>();
            float sqrHitWidth = _inGameParameter.DashDamageHitWidth * _inGameParameter.DashDamageHitWidth;
            foreach (var monster in _monsters)
            {
                if (GetSqrDistanceToSegment(monster.Position, start, end) <= sqrHitWidth)
                {
                    hitMonsters.Add(monster);
                }
            }

            foreach (var monster in hitMonsters)
            {
                monster.OnMonsterDamage(damage, BloodLuckDamageSource.Dash);
            }
        }

        private static float GetSqrDistanceToSegment(Vector3 point, Vector3 start, Vector3 end)
        {
            Vector3 segment = end - start;
            float sqrLength = segment.sqrMagnitude;
            float t = sqrLength > 0.0001f ? Mathf.Clamp01(Vector3.Dot(point - start, segment) / sqrLength) : 0f;
            return (point - (start + segment * t)).sqrMagnitude;
        }

        private BloodLuckMonster FindNearestMonster(Vector3 from, HashSet<BloodLuckMonster> excluded, float maxDistance)
        {
            BloodLuckMonster nearest = null;
            float nearestSqrDistance = maxDistance * maxDistance;
            foreach (var monster in _monsters)
            {
                if (excluded.Contains(monster))
                {
                    continue;
                }

                float sqrDistance = (monster.Position - from).sqrMagnitude;
                if (sqrDistance <= nearestSqrDistance)
                {
                    nearest = monster;
                    nearestSqrDistance = sqrDistance;
                }
            }

            return nearest;
        }

        private void OnAllMonsterDied()
        {
            _bloodLuckWaveManager.OnWaveFinished();
        }

        public void SpawnMonsters(BloodLuckWaveData wave)
        {
            Transform target = _player.transform;
            foreach (var entry in wave.Monsters)
            {
                for (int i = 0; i < entry.Count; i++)
                {
                    _monsters.Add(new BloodLuckMonster(
                        _assetHolder,
                        GetRandomSpawnPosition(),
                        entry,
                        _inGameParameter,
                        target,
                        _playerStateManager,
                        _effectManager,
                        _monsterProjectileManager,
                        this));
                }
            }
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