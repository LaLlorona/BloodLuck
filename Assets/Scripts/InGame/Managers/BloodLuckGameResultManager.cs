using Cysharp.Threading.Tasks;
using InGame.System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InGame.Managers
{
    public class BloodLuckGameResultManager : IBloodLuckContext
    {
        private readonly BloodLuckAssetHolder _assetHolder;
        private BloodLuckPlayerStateManager _playerStateManager;
        private BloodLuckWaveManager _waveManager;

        public BloodLuckGameResultManager(BloodLuckManagerParameter parameter)
        {
            _assetHolder = parameter.AssetHolder;
        }

        public UniTask InitAsync()
        {
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
            _playerStateManager = contextMap.GetRegisteredInstance<BloodLuckPlayerStateManager>();
            _waveManager = contextMap.GetRegisteredInstance<BloodLuckWaveManager>();
        }
        
        public void ShowResult(bool isVictory)
        {
            var view = Object.Instantiate(_assetHolder.GameResultViewPrefab);
            view.Show(
                isVictory,
                _waveManager.CurrentWaveNumber,
                _playerStateManager.CurrentPulls,
                _playerStateManager.MaxPulls,
                RestartGame);
        }

        private static void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
