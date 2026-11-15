using System;
using InGame.Managers;
using InGame.System;
using UnityEngine;

namespace InGame.Monsters
{
    public class BloodLuckMonster
    {
        private readonly BloodLuckMonsterComponent _monsterComponent;
        private readonly IBloodLuckMonsterBehaviour _behaviour;
        private readonly BloodLuckMonsterManager _monsterManager;
        private float _currentHp;
        private bool _isDead;

        public BloodLuckMonster(
            BloodLuckAssetHolder assetHolder,
            Vector3 spawnPosition,
            BloodLuckMonsterType monsterType,
            BloodLuckInGameParameter parameter,
            Transform player,
            BloodLuckPlayerStateManager playerStateManager,
            BloodLuckEffectManager effectManager,
            BloodLuckMonsterProjectileManager monsterProjectileManager,
            BloodLuckMonsterManager monsterManager)
        {
            _monsterManager = monsterManager;
            _currentHp = parameter.MonsterMaxHp;
            _monsterComponent = GameObject.Instantiate(
                assetHolder.GetBloodLuckMonsterComponent,
                spawnPosition,
                Quaternion.identity);
            _monsterComponent.Init(this);

            _behaviour = monsterType switch
            {
                BloodLuckMonsterType.Melee => new BloodLuckMeleeMonsterBehaviour(
                    _monsterComponent.transform,
                    parameter,
                    player,
                    playerStateManager,
                    effectManager),
                BloodLuckMonsterType.Ranged => new BloodLuckRangedMonsterBehaviour(
                    _monsterComponent.transform,
                    parameter,
                    player,
                    monsterProjectileManager),
                _ => throw new ArgumentOutOfRangeException(nameof(monsterType), monsterType, null),
            };
        }

        public void UpdateMonster(float deltaTime)
        {
            _behaviour.UpdateMonsterBehaviour(deltaTime);
            _monsterComponent.CustomUpdate(deltaTime);
        }

        public void OnMonsterDamage(float damage)
        {
            if (_isDead)
            {
                return;
            }

            _currentHp = Mathf.Max(0f, _currentHp - damage);
            if (_currentHp <= 0f)
            {
                _isDead = true;
                _monsterManager.OnMonsterDied(this);
                return;
            }

            _monsterComponent.PlayHitFlash();
        }

        public void OnMonsterDied()
        {
            _behaviour.OnMonsterDied();
            GameObject.Destroy(_monsterComponent.gameObject);
        }
    }
}
