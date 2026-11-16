using Cysharp.Threading.Tasks;
using InGame.System;
using UnityEngine;

namespace InGame.Managers
{
    public class BloodLuckShopManager : IBloodLuckContext
    {
        private readonly BloodLuckAssetHolder _assetHolder;
        private BloodLuckPlayerStateManager _playerStateManager;

        public BloodLuckShopManager(BloodLuckManagerParameter parameter)
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
        }

        /// <summary>Opens the shop and completes when the player leaves it. Purchases spend pulls and add passives.</summary>
        public async UniTask ShowShopAsync()
        {
            var shop = Object.Instantiate(_assetHolder.ShopViewPrefab);
            shop.Purchased += infusion => _playerStateManager.AddPassive(infusion.CreatePassive());
            try
            {
                await shop.OpenShopAsync(_playerStateManager, shop.destroyCancellationToken);
            }
            finally
            {
                if (shop != null)
                {
                    Object.Destroy(shop.gameObject);
                }
            }
        }
    }
}
