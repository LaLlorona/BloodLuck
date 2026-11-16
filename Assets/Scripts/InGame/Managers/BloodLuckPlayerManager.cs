using Cysharp.Threading.Tasks;
using InGame.System;

namespace InGame.Managers
{
    public class BloodLuckPlayerManager : IBloodLuckContext, IBloodLuckPriorityUpdatable
    {
        private PlayerAttack _playerAttack;
        private PlayerInputManager _playerInputManager;
        private PlayerMovement _playerMovement;

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
            _playerAttack.InitializePlayerAttack(
                contextMap.GetRegisteredInstance<BloodLuckEffectManager>(),
                contextMap.GetRegisteredInstance<BloodLuckPlayerStateManager>(),
                contextMap.GetRegisteredInstance<BloodLuckMonsterManager>());
        }
    }
}