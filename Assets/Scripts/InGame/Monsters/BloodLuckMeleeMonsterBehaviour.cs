using InGame.Effects;
using InGame.Managers;
using InGame.System;
using UnityEngine;

namespace InGame.Monsters
{
    public class BloodLuckMeleeMonsterBehaviour : IBloodLuckMonsterBehaviour
    {
        private enum State
        {
            Chasing,
            Charging,
            Waiting,
            Hit,
        }

        private readonly Transform _monsterTransform;
        private readonly BloodLuckInGameParameter _parameter;
        private readonly float _attackDamage;
        private readonly Transform _player;
        private readonly BloodLuckPlayerStateManager _playerStateManager;
        private readonly BloodLuckEffectManager _effectManager;

        private readonly BloodLuckMonsterHitStun _hitStun;
        private readonly BloodLuckMonsterAnimator _monsterAnimator;

        private State _state = State.Chasing;
        private BloodLuckSectorAreaEffectComponent _sectorEffect;
        private Vector2 _attackDirection;
        private float _chargeElapsed;
        private float _waitElapsed;
        private float _spawnAttackDelayRemaining;

        public BloodLuckMeleeMonsterBehaviour(
            Transform monsterTransform,
            BloodLuckInGameParameter parameter,
            float attackDamage,
            Transform player,
            BloodLuckPlayerStateManager playerStateManager,
            BloodLuckEffectManager effectManager,
            BloodLuckMonsterAnimator monsterAnimator)
        {
            _monsterAnimator = monsterAnimator;
            _monsterTransform = monsterTransform;
            _parameter = parameter;
            _attackDamage = attackDamage;
            _player = player;
            _playerStateManager = playerStateManager;
            _effectManager = effectManager;
            _hitStun = new BloodLuckMonsterHitStun(monsterTransform, player, parameter);
            _spawnAttackDelayRemaining = Random.Range(
                parameter.MonsterAttackStartTimeMin,
                parameter.MonsterAttackStarTimeMax);
        }

        public void UpdateMonsterBehaviour(float deltaTime)
        {
            if (_spawnAttackDelayRemaining > 0f)
            {
                _spawnAttackDelayRemaining -= deltaTime;
            }

            switch (_state)
            {
                case State.Chasing:
                    UpdateChasing(deltaTime);
                    break;
                case State.Charging:
                    UpdateCharging(deltaTime);
                    break;
                case State.Waiting:
                    UpdateWaiting(deltaTime);
                    break;
                case State.Hit:
                    UpdateHit(deltaTime);
                    break;
            }
        }

        public void ResetMonsterBehaviour()
        {
            if (_state == State.Charging)
            {
                RemoveSectorEffect();
            }

            _hitStun.Start();
            _state = State.Hit;
        }

        private void UpdateHit(float deltaTime)
        {
            if (_hitStun.Update(deltaTime))
            {
                _state = State.Chasing;
            }
        }

        public void OnMonsterDied()
        {
            if (_state == State.Charging)
            {
                RemoveSectorEffect();
            }
        }

        private void UpdateChasing(float deltaTime)
        {
            Vector3 playerPosition = _player.position;

            float distance = Vector2.Distance(_monsterTransform.position, playerPosition);
            if (_spawnAttackDelayRemaining <= 0f && distance <= _parameter.MonsterAttackTriggerDistance)
            {
                StartCharging(playerPosition);
                return;
            }

            _monsterTransform.position = Vector3.MoveTowards(
                _monsterTransform.position,
                playerPosition,
                _parameter.MonsterMoveSpeed * deltaTime);
        }

        private void StartCharging(Vector3 playerPosition)
        {
            Vector3 monsterPosition = _monsterTransform.position;
            _attackDirection = playerPosition - monsterPosition;
            _chargeElapsed = 0f;
            _state = State.Charging;
            _sectorEffect = _effectManager.ShowSectorArea(
                monsterPosition,
                _attackDirection,
                _parameter.MonsterAttackRadius,
                _parameter.MonsterAttackAngle);
        }

        private void UpdateCharging(float deltaTime)
        {
            _chargeElapsed += deltaTime;
            float chargeSeconds = _parameter.MonsterAttackChargeSeconds;
            _sectorEffect.SetProgress(_chargeElapsed / chargeSeconds);

            if (_chargeElapsed >= chargeSeconds)
            {
                Attack();
            }
        }

        private void Attack()
        {
            _monsterAnimator.PlayAttack();
            if (IsPlayerInsideSector())
            {
                BloodLuckDamageLogic.ApplyDamageToPlayer(_playerStateManager, _attackDamage);
            }

            RemoveSectorEffect();
            _waitElapsed = 0f;
            _state = State.Waiting;
        }

        private void UpdateWaiting(float deltaTime)
        {
            _waitElapsed += deltaTime;
            if (_waitElapsed >= _parameter.MonsterAttackWaitSeconds)
            {
                _state = State.Chasing;
            }
        }

        private bool IsPlayerInsideSector()
        {
            Vector2 toPlayer = _player.position - _monsterTransform.position;
            if (toPlayer.magnitude > _parameter.MonsterAttackRadius)
            {
                return false;
            }

            if (toPlayer.sqrMagnitude < 0.0001f)
            {
                return true;
            }

            return Vector2.Angle(_attackDirection, toPlayer) <= _parameter.MonsterAttackAngle * 0.5f;
        }

        private void RemoveSectorEffect()
        {
            _effectManager.RemoveSectorArea(_sectorEffect);
        }
    }
}
