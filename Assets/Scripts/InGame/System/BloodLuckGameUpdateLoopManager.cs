using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using InGame.Managers;
using UnityEngine;

namespace InGame.System
{
    public class BloodLuckGameUpdateLoopManager : IBloodLuckContext
    {
        private readonly List<IBloodLuckPriorityUpdatable> _updateList = new();
        private readonly Queue<BloodLuckAsyncTask> _asyncTasks = new();

        private BloodLuckGameStateManager _gameStateManager;
        private BloodLuckPerkSelectionManager _perkSelectionManager;
        private BloodLuckWaveManager _waveManager;
        private BloodLuckSlotMachineManager _slotMachineManager;
        private BloodLuckGameResultManager _gameResultManager;
        private BloodLuckShopManager _shopManager;
        private bool _isSorted;
        private bool _isHandlingAsyncTask;

        public UniTask InitAsync()
        {
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
            _gameStateManager = contextMap.GetRegisteredInstance<BloodLuckGameStateManager>();
            _perkSelectionManager = contextMap.GetRegisteredInstance<BloodLuckPerkSelectionManager>();
            _waveManager = contextMap.GetRegisteredInstance<BloodLuckWaveManager>();
            _slotMachineManager = contextMap.GetRegisteredInstance<BloodLuckSlotMachineManager>();
            _gameResultManager = contextMap.GetRegisteredInstance<BloodLuckGameResultManager>();
            _shopManager = contextMap.GetRegisteredInstance<BloodLuckShopManager>();
        }

        public void RegisterUpdatable(IBloodLuckPriorityUpdatable updatable)
        {
            _updateList.Add(updatable);
            _isSorted = false;
        }

        public void AddAsyncTask(BloodLuckAsyncTask task)
        {
            _asyncTasks.Enqueue(task);
        }

        public void UpdateInGameLoop(float deltaTime)
        {
            if (_gameStateManager.CurrentGameState is not BloodLuckGameStateManager.GameState.GameProceeding)
            {
                return;
            }

            if (_isHandlingAsyncTask)
            {
                return;
            }

            if (_asyncTasks.Count > 0)
            {
                HandleAsyncTasks().Forget();
            }
            else
            {
                UpdateAll(deltaTime);
            }
        }

        private void UpdateAll(float deltaTime)
        {
            if (!_isSorted)
            {
                _updateList.Sort((a, b) => a.UpdatePriority.CompareTo(b.UpdatePriority));
                _isSorted = true;
            }

            foreach (var updatable in _updateList)
            {
                updatable.GameUpdate(deltaTime);
            }
        }

        private async UniTaskVoid HandleAsyncTasks()
        {
            _isHandlingAsyncTask = true;
            try
            {
                while (_asyncTasks.TryDequeue(out var task))
                {
                    if (_gameStateManager.CurrentGameState is BloodLuckGameStateManager.GameState.GameFinished)
                    {
                        // Once the game has finished, ignore all remaining async tasks.
                        continue;
                    }

                    await HandleAsyncTask(task);
                }
            }
            finally
            {
                _isHandlingAsyncTask = false;
            }
        }

        private UniTask HandleAsyncTask(BloodLuckAsyncTask task)
        {
            switch (task)
            {
                case BloodLuckAsyncTask.LastWaveSlotMachine:
                    return HandleGameFinish();
                case BloodLuckAsyncTask.WaveFinishedAndSelectingPerk:
                    return HandleWaveFinishedAndSelectingPerk();
                case BloodLuckAsyncTask.WaveFinishedAndVisitingShop:
                    return HandleVisitingShop();
                default:
                    throw new ArgumentOutOfRangeException(nameof(task), task, "Unhandled BloodLuckAsyncTask.");
            }
        }

        private async UniTask HandleVisitingShop()
        {
            _gameStateManager.PauseGame();
            await _shopManager.ShowShopAsync();
            _gameStateManager.ResumeGame();
            _waveManager.OnWaveStarted();
        }

        private async UniTask HandleWaveFinishedAndSelectingPerk()
        {
            _gameStateManager.PauseGame();
            await _perkSelectionManager.ShowPerkSelectionAsync();
            _gameStateManager.ResumeGame();
            _waveManager.OnWaveStarted();
        }

        private async UniTask HandleGameFinish()
        {
            _gameStateManager.PauseGame();
            bool isJackpot = await _slotMachineManager.PlaySlotMachineAsync();
            _gameStateManager.FinishGame();
            _gameResultManager.ShowResult(isJackpot);
        }
    }
}
