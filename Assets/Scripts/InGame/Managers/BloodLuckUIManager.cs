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
        private BloodLuckHudViewComponent _hudView;

        public BloodLuckUIManager(BloodLuckAssetHolder assetHolder)
        {
            _assetHolder = assetHolder;
        }

        public UniTask InitAsync()
        {
            _hudView = Object.Instantiate(_assetHolder.HudViewPrefab);
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
            _playerStateManager = contextMap.GetRegisteredInstance<BloodLuckPlayerStateManager>();
            RefreshHud();
        }

        public void RefreshHud()
        {
            _hudView.SetHp(_playerStateManager.CurrentHp, _playerStateManager.MaxHp);
            _hudView.SetPulls(_playerStateManager.CurrentPulls, _playerStateManager.MaxPulls);
        }
    }
}
