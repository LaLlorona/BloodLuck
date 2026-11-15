using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using InGame.Projectiles;
using InGame.System;
using UnityEngine;

namespace InGame.Managers
{
    public class BloodLuckMonsterProjectileManager : IBloodLuckContext, IBloodLuckPriorityUpdatable
    {
        private class MonsterProjectile
        {
            public BloodLuckProjectileComponent Component;
            public Vector2 Direction;
            public float Speed;
            public float Damage;
            public float HitRadius;
            public float RemainingLifetime;
        }

        private readonly BloodLuckAssetHolder _assetHolder;
        private readonly List<MonsterProjectile> _monsterProjectiles = new();
        private BloodLuckPlayerStateManager _playerStateManager;
        private PlayerMovement _player;

        public BloodLuckMonsterProjectileManager(BloodLuckAssetHolder assetHolder)
        {
            _assetHolder = assetHolder;
        }

        public UniTask InitAsync()
        {
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
            contextMap.GetRegisteredInstance<BloodLuckGameUpdateLoopManager>().RegisterUpdatable(this);
            _playerStateManager = contextMap.GetRegisteredInstance<BloodLuckPlayerStateManager>();
            _player = Object.FindAnyObjectByType<PlayerMovement>();
        }

        public BloodLuckUpdatePriority UpdatePriority => BloodLuckUpdatePriority.ProjectileUpdate;

        public void SpawnMonsterProjectile(
            Vector3 position,
            Vector2 direction,
            float speed,
            float damage,
            float hitRadius,
            float lifetime)
        {
            var component = Object.Instantiate(
                _assetHolder.ProjectilePrefab,
                position,
                Quaternion.identity);

            _monsterProjectiles.Add(new MonsterProjectile
            {
                Component = component,
                Direction = direction.normalized,
                Speed = speed,
                Damage = damage,
                HitRadius = hitRadius,
                RemainingLifetime = lifetime,
            });
        }

        public void GameUpdate(float deltaTime)
        {
            for (int i = _monsterProjectiles.Count - 1; i >= 0; i--)
            {
                var projectile = _monsterProjectiles[i];
                var projectileTransform = projectile.Component.transform;
                projectileTransform.position += (Vector3)(projectile.Direction * (projectile.Speed * deltaTime));
                projectile.RemainingLifetime -= deltaTime;

                float distanceToPlayer = Vector2.Distance(projectileTransform.position, _player.transform.position);
                if (distanceToPlayer <= projectile.HitRadius)
                {
                    BloodLuckDamageLogic.ApplyDamageToPlayer(_playerStateManager, projectile.Damage);
                    RemoveProjectileAt(i);
                }
                else if (projectile.RemainingLifetime <= 0f)
                {
                    RemoveProjectileAt(i);
                }
            }
        }

        private void RemoveProjectileAt(int index)
        {
            Object.Destroy(_monsterProjectiles[index].Component.gameObject);
            _monsterProjectiles.RemoveAt(index);
        }
    }
}
