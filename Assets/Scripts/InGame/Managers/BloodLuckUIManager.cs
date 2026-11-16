using Cysharp.Threading.Tasks;
using InGame.System;
using InGame.UI;
using UnityEngine;

namespace InGame.Managers
{
    public class BloodLuckUIManager : IBloodLuckContext
    {
        private readonly BloodLuckAssetHolder _assetHolder;
        private BloodLuckPlayerStateManager _playerStateManager;
        private BloodLuckWaveManager _waveManager;
        private BloodLuckHudViewComponent _hudView;

        public BloodLuckUIManager(BloodLuckManagerParameter parameter)
        {
            _assetHolder = parameter.AssetHolder;
        }

        public UniTask InitAsync()
        {
            _hudView = Object.Instantiate(_assetHolder.HudViewPrefab);
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
            _playerStateManager = contextMap.GetRegisteredInstance<BloodLuckPlayerStateManager>();
            _waveManager = contextMap.GetRegisteredInstance<BloodLuckWaveManager>();
            RefreshHud();
        }

        public void RefreshHud()
        {
            _hudView.SetWave(_waveManager.CurrentWaveNumber);
            _hudView.SetHp(_playerStateManager.CurrentHp, _playerStateManager.MaxHp);
            _hudView.SetPulls(_playerStateManager.CurrentPulls, _playerStateManager.MaxPulls);
        }
    }
}
