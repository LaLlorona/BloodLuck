using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using InGame.System;
using InGame.UI;
using UnityEngine;

namespace InGame.Managers
{
    public class BloodLuckPerkSelectionManager : IBloodLuckContext
    {
        private readonly BloodLuckAssetHolder _assetHolder;

        public BloodLuckPerkSelectionManager(BloodLuckAssetHolder assetHolder)
        {
            _assetHolder = assetHolder;
        }

        public UniTask InitAsync()
        {
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
        }
        
        public async UniTask<PerkData> ShowPerkSelectionAsync()
        {
            var completionSource = new UniTaskCompletionSource<PerkData>();

            // The prefab root has its own Canvas, so it does not need a parent.
            var view = Object.Instantiate(_assetHolder.PerkSelectionViewPrefab);
            try
            {
                view.Show(GetPerkChoices(), perk => completionSource.TrySetResult(perk));
                return await completionSource.Task;
            }
            finally
            {
                // Destroy the view even if the task is cancelled or throws.
                if (view != null)
                {
                    Object.Destroy(view.gameObject);
                }
            }
        }

        private static IReadOnlyList<PerkData> GetPerkChoices()
        {
            return new List<PerkData>
            {
                new(null, "Dummy Perk 1", "Dummy description 1"),
                new(null, "Dummy Perk 2", "Dummy description 2"),
                new(null, "Dummy Perk 3", "Dummy description 3"),
            };
        }
    }
}
