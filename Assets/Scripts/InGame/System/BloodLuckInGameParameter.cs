using System;
using UnityEngine;

namespace InGame.System
{
    [Serializable]
    public class BloodLuckInGameParameter
    {
        [SerializeField] private float _monsterSpawnRadius = 5;
        [SerializeField, Min(0f)] private float _monsterMoveSpeed = 2f;

        [Header("Monster HP Trail")]
        [Tooltip("Seconds the white trail waits at the old HP after the first damage.")]
        [SerializeField, Min(0f)] private float _hpTrailHoldSeconds = 0.4f;
        [Tooltip("Minimum drain speed of the trail (hp ratio per second).")]
        [SerializeField, Min(0f)] private float _hpTrailMinSpeed = 0.3f;
        [Tooltip("Extra drain speed per unit of gap, so big bursts drain faster.")]
        [SerializeField, Min(0f)] private float _hpTrailSpeedPerGap = 2f;

        [Header("Monster Attack (sector)")]
        [Tooltip("When the monster gets this close to the player, it stops and starts charging its attack.")]
        [SerializeField, Min(0f)] private float _monsterAttackTriggerDistance = 2f;
        [Tooltip("Seconds the sector is shown before the damage is applied.")]
        [SerializeField, Min(0.01f)] private float _monsterAttackChargeSeconds = 1.5f;
        [Tooltip("Seconds the monster stays idle after its attack before chasing again.")]
        [SerializeField, Min(0f)] private float _monsterAttackWaitSeconds = 1f;
        [SerializeField, Min(0.1f)] private float _monsterAttackRadius = 3f;
        [SerializeField, Range(1f, 360f)] private float _monsterAttackAngle = 90f;

        [Header("Monster Ranged Attack")]
        [Tooltip("The ranged monster fires when the player is within this distance.")]
        [SerializeField, Min(0f)] private float _monsterRangedAttackRange = 6f;
        [Tooltip("Seconds the ranged monster stands still after firing.")]
        [SerializeField, Min(0f)] private float _monsterRangedStopSeconds = 1.5f;
        [SerializeField, Min(0f)] private float _monsterProjectileSpeed = 6f;
        [SerializeField, Min(0.01f)] private float _monsterProjectileHitRadius = 0.4f;
        [SerializeField, Min(0.1f)] private float _monsterProjectileLifetime = 5f;

        [Header("Player")]
        [SerializeField, Min(1f)] private float _playerMaxHp = 100f;
        [SerializeField, Min(0)] private int _playerMaxPulls = 3;
        [SerializeField, Min(0f)] private float _playerAttack = 10f;
        [SerializeField, Min(0f)] private float _playerAttackSpeed = 1f;

        [Header("Slot Machine")]
        [Range(0f, 1f)] [SerializeField] private float _slotJackpotChance = 0.2f;

        public float HpTrailHoldSeconds => _hpTrailHoldSeconds;
        public float HpTrailMinSpeed => _hpTrailMinSpeed;
        public float HpTrailSpeedPerGap => _hpTrailSpeedPerGap;
        public float SlotJackpotChance => _slotJackpotChance;
        public float MonsterRangedAttackRange => _monsterRangedAttackRange;
        public float MonsterRangedStopSeconds => _monsterRangedStopSeconds;
        public float MonsterProjectileSpeed => _monsterProjectileSpeed;
        public float MonsterProjectileHitRadius => _monsterProjectileHitRadius;
        public float MonsterProjectileLifetime => _monsterProjectileLifetime;

        public float PlayerMaxHp => _playerMaxHp;
        public int PlayerMaxPulls => _playerMaxPulls;
        public float PlayerAttack => _playerAttack;
        public float PlayerAttackSpeed => _playerAttackSpeed;

        public float MonsterSpawnRadius => _monsterSpawnRadius;
        public float MonsterMoveSpeed => _monsterMoveSpeed;
        public float MonsterAttackTriggerDistance => _monsterAttackTriggerDistance;
        public float MonsterAttackChargeSeconds => _monsterAttackChargeSeconds;
        public float MonsterAttackWaitSeconds => _monsterAttackWaitSeconds;
        public float MonsterAttackRadius =>_monsterAttackRadius;
        public float MonsterAttackAngle => _monsterAttackAngle;
    }
}
