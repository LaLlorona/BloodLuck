using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using InGame.System;
using InGame.UI;
using Player;
using UnityEngine;

namespace InGame.Managers
{
    public class BloodLuckPerkSelectionManager : IBloodLuckContext
    {
        private readonly BloodLuckAssetHolder _assetHolder;
        private readonly BloodLuckInGameParameter _inGameParameter;
        private readonly global::System.Random _random = new();
        private BloodLuckPlayerStateManager _playerStateManager;

        public BloodLuckPerkSelectionManager(BloodLuckManagerParameter parameter)
        {
            _assetHolder = parameter.AssetHolder;
            _inGameParameter = parameter.InGameParameter;
        }

        public UniTask InitAsync()
        {
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
            _playerStateManager = contextMap.GetRegisteredInstance<BloodLuckPlayerStateManager>();
        }
        
        public async UniTask<PerkData> ShowPerkSelectionAsync()
        {
            var completionSource = new UniTaskCompletionSource<PerkData>();

            var view = Object.Instantiate(_assetHolder.PerkSelectionViewPrefab);
            try
            {
                view.Show(GetPerkChoices(), perk =>
                {
                    _playerStateManager.AddPassive(perk.Passive);
                    completionSource.TrySetResult(perk);
                });
                return await completionSource.Task;
            }
            finally
            {
                Object.Destroy(view.gameObject);
            }
        }

        private IReadOnlyList<PerkData> GetPerkChoices()
        {
            var perks = new List<PerkData>();
            for (int i = 0; i < 3; i++)
            {
                var passive = PlayerPassiveGenerator.GetRandomPlayerPassive(_random);
                perks.Add(new PerkData(_assetHolder.GetPerkIcon(passive.IconName), passive));
            }

            return perks;
        }
    }
}
