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

        public BloodLuckMonster(
            BloodLuckAssetHolder assetHolder,
            Vector3 spawnPosition,
            BloodLuckMonsterType monsterType,
            BloodLuckInGameParameter parameter,
            Transform player,
            BloodLuckPlayerStateManager playerStateManager,
            BloodLuckEffectManager effectManager,
            BloodLuckMonsterProjectileManager monsterProjectileManager)
        {
            _monsterComponent = GameObject.Instantiate(
                assetHolder.GetBloodLuckMonsterComponent,
                spawnPosition,
                Quaternion.identity);

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
        }

        public void OnMonsterDied()
        {
            _behaviour.OnMonsterDied();
            GameObject.Destroy(_monsterComponent.gameObject);
        }
    }
}
