using InGame.Managers;
using InGame.System;
using UnityEngine;

namespace InGame.Monsters
{
    public class BloodLuckRangedMonsterBehaviour : IBloodLuckMonsterBehaviour
    {
        private enum State
        {
            Chasing,
            Stopped,
            Hit,
        }

        private readonly Transform _monsterTransform;
        private readonly BloodLuckInGameParameter _parameter;
        private readonly float _projectileDamage;
        private readonly Transform _player;
        private readonly BloodLuckMonsterProjectileManager _monsterProjectileManager;

        private readonly BloodLuckMonsterHitStun _hitStun;

        private State _state = State.Chasing;
        private float _stoppedElapsed;

        public BloodLuckRangedMonsterBehaviour(
            Transform monsterTransform,
            BloodLuckInGameParameter parameter,
            float projectileDamage,
            Transform player,
            BloodLuckMonsterProjectileManager monsterProjectileManager)
        {
            _monsterTransform = monsterTransform;
            _parameter = parameter;
            _projectileDamage = projectileDamage;
            _player = player;
            _monsterProjectileManager = monsterProjectileManager;
            _hitStun = new BloodLuckMonsterHitStun(monsterTransform, player, parameter);
        }

        public void UpdateMonsterBehaviour(float deltaTime)
        {
            switch (_state)
            {
                case State.Chasing:
                    UpdateChasing(deltaTime);
                    break;
                case State.Stopped:
                    UpdateStopped(deltaTime);
                    break;
                case State.Hit:
                    UpdateHit(deltaTime);
                    break;
            }
        }

        public void ResetMonsterBehaviour()
        {
            _hitStun.Start();
            _state = State.Hit;
        }

        public void OnMonsterDied()
        {
        }

        private void UpdateHit(float deltaTime)
        {
            if (_hitStun.Update(deltaTime))
            {
                _state = State.Chasing;
            }
        }

        private void UpdateChasing(float deltaTime)
        {
            Vector3 playerPosition = _player.position;
            float distance = Vector2.Distance(_monsterTransform.position, playerPosition);
            if (distance <= _parameter.MonsterRangedAttackRange)
            {
                FireProjectile(playerPosition);
                return;
            }

            _monsterTransform.position = Vector3.MoveTowards(
                _monsterTransform.position,
                playerPosition,
                _parameter.MonsterMoveSpeed * deltaTime);
        }

        private void FireProjectile(Vector3 playerPosition)
        {
            Vector3 monsterPosition = _monsterTransform.position;
            _monsterProjectileManager.SpawnMonsterProjectile(
                monsterPosition,
                playerPosition - monsterPosition,
                _parameter.MonsterProjectileSpeed,
                _projectileDamage,
                _parameter.MonsterProjectileHitRadius,
                _parameter.MonsterProjectileLifetime);

            _stoppedElapsed = 0f;
            _state = State.Stopped;
        }

        private void UpdateStopped(float deltaTime)
        {
            _stoppedElapsed += deltaTime;
            if (_stoppedElapsed >= _parameter.MonsterRangedStopSeconds)
            {
                _state = State.Chasing;
            }
        }
    }
}
