using Cysharp.Threading.Tasks;
using BloodLuck;
using InGame.System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InGame.Managers
{
    public class BloodLuckGameResultManager : IBloodLuckContext
    {
        private readonly BloodLuckAssetHolder _assetHolder;
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
            _waveManager = contextMap.GetRegisteredInstance<BloodLuckWaveManager>();
        }
        
        public void ShowResult(BloodLuckGameResultType resultType)
        {
            if (resultType == BloodLuckGameResultType.EscapedCasino)
            {
                AudioManager.Instance?.PlayWinMusic();
                AudioManager.Instance?.PlayWinSfx();
            }
            else if (resultType == BloodLuckGameResultType.LostAtSlotMachine)
            {
                AudioManager.Instance?.PlayDefeatMusic();
            }

            var view = Object.Instantiate(_assetHolder.GameResultViewPrefab);
            view.Show(
                resultType,
                _waveManager.CurrentWaveNumber,
                RestartGame);
        }

        private static void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
