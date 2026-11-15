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
            if (Keyboard.current.digit0Key.wasPressedThisFrame)
            {
                _monsterManager.CheatKillAllMonsters();
            }
        }
    }
}