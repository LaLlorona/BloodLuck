using Cysharp.Threading.Tasks;
using InGame.System;
using UnityEngine.InputSystem;

namespace InGame.Managers
{
    public class BloodLuckCheatManager : IBloodLuckContext, IBloodLuckPriorityUpdatable
    {
        private BloodLuckMonsterManager _monsterManager;
        private BloodLuckGameUpdateLoopManager _updateLoopManager;
        public UniTask InitAsync()
        {
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
            _monsterManager = contextMap.GetRegisteredInstance<BloodLuckMonsterManager>();
            _updateLoopManager = contextMap.GetRegisteredInstance<BloodLuckGameUpdateLoopManager>();
            _updateLoopManager.RegisterUpdatable(this);
        }

        public BloodLuckUpdatePriority UpdatePriority => BloodLuckUpdatePriority.InputUpdate;
        public void GameUpdate(float deltaTime)
        {
            // The project uses the Input System package, so the legacy UnityEngine.Input class cannot be used.
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.digit0Key.wasPressedThisFrame)
            {
                _monsterManager.CheatKillAllMonsters();
            }
        }
    }
}