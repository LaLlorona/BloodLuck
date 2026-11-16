using Cysharp.Threading.Tasks;
using InGame.System;
using UnityEngine;

namespace InGame.Managers
{
    public class BloodLuckPlayerManager : IBloodLuckContext, IBloodLuckPriorityUpdatable
    {
        private PlayerAttack _playerAttack;
        private PlayerInputManager _playerInputManager;
        private PlayerMovement _playerMovement;
        private BloodLuckEffectManager _effectManager;
        private BloodLuckPlayerStateManager _playerStateManager;
        private BloodLuckMonsterManager _monsterManager;

        public BloodLuckUpdatePriority UpdatePriority => BloodLuckUpdatePriority.InputUpdate;

        public BloodLuckPlayerManager(BloodLuckManagerParameter parameter)
        {
            _playerAttack = parameter.AssetHolder.GetPlayerAttackComponent;
            _playerInputManager = parameter.AssetHolder.GetPlayerInputManagerComponent;
            _playerMovement = parameter.AssetHolder.GetPlayerMovementComponent;
        }

        public void GameUpdate(float deltaTime)
        {
            _playerInputManager.GameUpdate();
            _playerAttack.GameUpdate(deltaTime);
            _playerMovement.GameUpdate(deltaTime);
        }

        public UniTask InitAsync()
        {
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
            var updateLoopManager = contextMap.GetRegisteredInstance<BloodLuckGameUpdateLoopManager>();
            updateLoopManager.RegisterUpdatable(this);
            _effectManager = contextMap.GetRegisteredInstance<BloodLuckEffectManager>();
            _playerStateManager = contextMap.GetRegisteredInstance<BloodLuckPlayerStateManager>();
            _monsterManager = contextMap.GetRegisteredInstance<BloodLuckMonsterManager>();
            _playerAttack.InitializePlayerAttack(_effectManager, _playerStateManager, _monsterManager);
            _playerMovement.InitializePlayerMovement(this);
        }

        public void HandlePlayerDash(Vector3 start, Vector3 end)
        {
            float dashDamage = _playerStateManager.DashDamage;
            if (dashDamage <= 0f)
            {
                return;
            }

            _effectManager.ShowChainLightning(new[] { start, end });
            _monsterManager.ApplyDashDamage(start, end, dashDamage);
        }
    }
}