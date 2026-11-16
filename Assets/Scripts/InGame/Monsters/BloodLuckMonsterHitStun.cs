using InGame.System;
using UnityEngine;

namespace InGame.Monsters
{
    public class BloodLuckMonsterHitStun
    {
        private readonly Transform _monsterTransform;
        private readonly Transform _player;
        private readonly BloodLuckInGameParameter _parameter;

        private Vector3 _knockbackDirection;
        private float _elapsed;

        public BloodLuckMonsterHitStun(
            Transform monsterTransform,
            Transform player,
            BloodLuckInGameParameter parameter)
        {
            _monsterTransform = monsterTransform;
            _player = player;
            _parameter = parameter;
        }

        public void Start()
        {
            Vector3 away = _monsterTransform.position - _player.position;
            away.z = 0f;
            _knockbackDirection = away.sqrMagnitude < 0.0001f ? Vector3.right : away.normalized;
            _elapsed = 0f;
        }

        // Returns true once the stun is over.
        public bool Update(float deltaTime)
        {
            float knockbackSeconds = _parameter.MonsterKnockbackSeconds;
            float previous = _elapsed;
            _elapsed += deltaTime;

            if (knockbackSeconds > 0f && previous < knockbackSeconds)
            {
                float step = Mathf.Min(_elapsed, knockbackSeconds) - previous;
                float speed = _parameter.MonsterKnockbackDistance / knockbackSeconds;
                _monsterTransform.position += _knockbackDirection * (speed * step);
            }

            return _elapsed >= _parameter.MonsterHitStunSeconds;
        }
    }
}
