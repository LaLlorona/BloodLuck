using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using InGame.Effects;
using InGame.System;
using UnityEngine;

namespace InGame.Managers
{
    public class BloodLuckEffectManager : IBloodLuckContext
    {
        private readonly BloodLuckAssetHolder _assetHolder;
        private readonly List<BloodLuckSectorAreaEffectComponent> _sectorAreaEffects = new();

        public BloodLuckEffectManager(BloodLuckAssetHolder assetHolder)
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

        public BloodLuckSectorAreaEffectComponent ShowSectorArea(
            Vector3 position,
            Vector2 direction,
            float radius,
            float angleDegrees)
        {
            var effect = Object.Instantiate(
                _assetHolder.SectorAreaEffectPrefab,
                position,
                Quaternion.identity);
            effect.Setup(radius, angleDegrees);
            effect.SetDirection(direction);
            _sectorAreaEffects.Add(effect);
            return effect;
        }

        public void RemoveSectorArea(BloodLuckSectorAreaEffectComponent effect)
        {
            _sectorAreaEffects.Remove(effect);
            Object.Destroy(effect.gameObject);
        }
    }
}
