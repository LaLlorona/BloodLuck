using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using InGame.Managers;

namespace InGame.System
{
    public class BloodLuckGameUpdateLoopManager : IBloodLuckContext
    {
        private readonly List<IBloodLuckPriorityUpdatable> _updateList = new();
        private readonly Queue<BloodLuckAsyncTask> _asyncTasks = new();

        private BloodLuckGameStateManager _gameStateManager;
        private BloodLuckPerkSelectionManager _perkSelectionManager;
        private BloodLuckWaveManager _waveManager;
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
                case BloodLuckAsyncTask.HandleGameFinish:
                    return HandleGameFinish();
                case BloodLuckAsyncTask.WaveFinishedAndSelectingPerk:
                    return HandleWaveFinishedAndSelectingPerk();
                default:
                    throw new ArgumentOutOfRangeException(nameof(task), task, "Unhandled BloodLuckAsyncTask.");
            }
        }

        private async UniTask HandleWaveFinishedAndSelectingPerk()
        {
            _gameStateManager.PauseGame();

            var selectedPerk = await _perkSelectionManager.ShowPerkSelectionAsync();
            // ToDo: add the selected perk to the player once the player system exists.

            _gameStateManager.ResumeGame();
            _waveManager.OnWaveStarted();
        }

        private UniTask HandleGameFinish()
        {
            _gameStateManager.FinishGame();
            return UniTask.CompletedTask;
        }
    }
}
