using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using InGame.Effects;
using InGame.System;
using UnityEngine;

namespace InGame.Managers
{
    public class BloodLuckEffectManager : IBloodLuckContext, IBloodLuckPriorityUpdatable
    {
        private readonly BloodLuckAssetHolder _assetHolder;
        private readonly List<BloodLuckSectorAreaEffectComponent> _sectorAreaEffects = new();
        private readonly List<SlashPlayback> _slashPlaybacks = new();

        private sealed class SlashPlayback
        {
            public BloodLuckSlashEffectComponent Effect;
            public float Elapsed;
        }

        public BloodLuckUpdatePriority UpdatePriority => BloodLuckUpdatePriority.EffectUpdate;

        public BloodLuckEffectManager(BloodLuckManagerParameter parameter)
        {
            _assetHolder = parameter.AssetHolder;
        }

        public UniTask InitAsync()
        {
            return UniTask.CompletedTask;
        }

        public void WireContextAfterInit(BloodLuckContextMap contextMap)
        {
            contextMap.GetRegisteredInstance<BloodLuckGameUpdateLoopManager>().RegisterUpdatable(this);
        }

        public void GameUpdate(float deltaTime)
        {
            for (int i = _slashPlaybacks.Count - 1; i >= 0; i--)
            {
                var playback = _slashPlaybacks[i];
                if (playback.Effect == null)
                {
                    _slashPlaybacks.RemoveAt(i);
                    continue;
                }

                playback.Elapsed += deltaTime;
                float normalizedTime = playback.Elapsed / playback.Effect.Duration;
                playback.Effect.SetNormalizedTime(normalizedTime);

                if (normalizedTime >= 1f)
                {
                    Object.Destroy(playback.Effect.gameObject);
                    _slashPlaybacks.RemoveAt(i);
                }
            }
        }

        public BloodLuckSlashEffectComponent PlaySlash(
            Vector3 position,
            Vector2 direction,
            float radius,
            float angleDegrees,
            Color color,
            bool clockwise = false)
        {
            var effect = Object.Instantiate(
                _assetHolder.SlashEffectPrefab,
                position,
                Quaternion.identity);
            effect.SetColor(color);
            effect.Setup(radius, angleDegrees);
            effect.SetDirection(direction);
            effect.SetClockwise(clockwise);
            effect.SetNormalizedTime(0f);
            _slashPlaybacks.Add(new SlashPlayback { Effect = effect });
            return effect;
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
