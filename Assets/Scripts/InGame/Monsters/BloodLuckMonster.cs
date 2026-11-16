using System;
using InGame.Managers;
using InGame.System;
using UnityEngine;

namespace InGame.Monsters
{
    public class BloodLuckMonster
    {
        private readonly BloodLuckMonsterComponent _monsterComponent;
        private readonly Transform _player;
        private readonly IBloodLuckMonsterBehaviour _behaviour;
        private readonly BloodLuckMonsterManager _monsterManager;
        private readonly BloodLuckPlayerStateManager _playerStateManager;
        private readonly float _maxHp;
        private float _currentHp;
        private bool _isDead;

        public BloodLuckMonster(
            BloodLuckAssetHolder assetHolder,
            Vector3 spawnPosition,
            BloodLuckWaveMonsterEntry spawnData,
            BloodLuckInGameParameter parameter,
            Transform player,
            BloodLuckPlayerStateManager playerStateManager,
            BloodLuckEffectManager effectManager,
            BloodLuckMonsterProjectileManager monsterProjectileManager,
            BloodLuckMonsterManager monsterManager)
        {
            _player = player;
            _monsterManager = monsterManager;
            _playerStateManager = playerStateManager;
            _maxHp = spawnData.MaxHp;
            _currentHp = _maxHp;
            _monsterComponent = GameObject.Instantiate(
                assetHolder.GetBloodLuckMonsterComponent,
                spawnPosition,
                Quaternion.identity);
            _monsterComponent.Init(this, parameter);
            _monsterComponent.SetHp(_currentHp, _maxHp);

            _behaviour = spawnData.MonsterType switch
            {
                BloodLuckMonsterType.Melee => new BloodLuckMeleeMonsterBehaviour(
                    _monsterComponent.transform,
                    parameter,
                    spawnData.BaseDamage,
                    player,
                    playerStateManager,
                    effectManager,
                    _monsterComponent.MonsterAnimator),
                BloodLuckMonsterType.Ranged => new BloodLuckRangedMonsterBehaviour(
                    _monsterComponent.transform,
                    parameter,
                    spawnData.BaseDamage,
                    player,
                    monsterProjectileManager),
                _ => throw new ArgumentOutOfRangeException(nameof(spawnData.MonsterType), spawnData.MonsterType, null),
            };
        }

        public Vector3 Position => _monsterComponent.transform.position;

        public void UpdateMonster(float deltaTime)
        {
            _behaviour.UpdateMonsterBehaviour(deltaTime);
            _monsterComponent.FaceHorizontal(_player.position.x - Position.x);
            _monsterComponent.CustomUpdate(deltaTime);
        }

        public void OnMonsterDamage(float damage, BloodLuckDamageSource source)
        {
            if (_isDead)
            {
                return;
            }

            _playerStateManager.HealFromDamageDealt(damage);
            if (source == BloodLuckDamageSource.Attack)
            {
                _monsterManager.ApplyChainLightning(this, _playerStateManager.ChainLightningDamage);
            }

            _currentHp = Mathf.Max(0f, _currentHp - damage);
            if (_currentHp <= 0f)
            {
                _isDead = true;
                _monsterManager.OnMonsterDied(this);
                return;
            }

            _monsterComponent.SetHp(_currentHp, _maxHp);
            _monsterComponent.PlayHitFlash();
            _behaviour.ResetMonsterBehaviour();
        }

        public void OnMonsterDied()
        {
            _behaviour.OnMonsterDied();
            GameObject.Destroy(_monsterComponent.gameObject);
        }
    }
}
