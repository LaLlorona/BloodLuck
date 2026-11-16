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

        private sealed class LightningPlayback
        {
            public LineRenderer Glow;
            public LineRenderer Core;
            public Vector3[] AnchorPoints;
            public float Elapsed;
            public float FlickerElapsed;
        }

        private readonly List<Vector3> _zigzagBuffer = new();

        private readonly BloodLuckInGameParameter _inGameParameter;
        private readonly List<LightningPlayback> _lightningPlaybacks = new();
        private Material _fallbackLineMaterial;

        public BloodLuckUpdatePriority UpdatePriority => BloodLuckUpdatePriority.EffectUpdate;

        public BloodLuckEffectManager(BloodLuckManagerParameter parameter)
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
            contextMap.GetRegisteredInstance<BloodLuckGameUpdateLoopManager>().RegisterUpdatable(this);
        }

        public void GameUpdate(float deltaTime)
        {
            UpdateLightningPlaybacks(deltaTime);

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

        public void ShowChainLightning(IReadOnlyList<Vector3> points)
        {
            var root = new GameObject("ChainLightning");
            var playback = new LightningPlayback
            {
                Glow = CreateLightningLine(
                    root.transform,
                    "Glow",
                    _inGameParameter.ChainLightningLineWidth * _inGameParameter.ChainLightningGlowWidthScale,
                    30),
                Core = CreateLightningLine(
                    root.transform,
                    "Core",
                    _inGameParameter.ChainLightningLineWidth,
                    31),
                AnchorPoints = new Vector3[points.Count],
            };

            for (int i = 0; i < points.Count; i++)
            {
                playback.AnchorPoints[i] = points[i];
            }

            ApplyZigzag(playback);
            ApplyLightningColor(playback, 0f);
            _lightningPlaybacks.Add(playback);
        }

        private LineRenderer CreateLightningLine(Transform parent, string objectName, float width, int sortingOrder)
        {
            var child = new GameObject(objectName);
            child.transform.SetParent(parent, false);

            var line = child.AddComponent<LineRenderer>();
            line.sharedMaterial = GetLightningLineMaterial();
            line.useWorldSpace = true;
            line.widthMultiplier = width;
            line.numCornerVertices = 2;
            line.numCapVertices = 2;
            line.sortingOrder = sortingOrder;
            line.widthCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.5f, 0.7f),
                new Keyframe(1f, 1f));
            return line;
        }

        private void ApplyZigzag(LightningPlayback playback)
        {
            _zigzagBuffer.Clear();
            var anchors = playback.AnchorPoints;
            _zigzagBuffer.Add(anchors[0]);

            for (int i = 0; i < anchors.Length - 1; i++)
            {
                Vector3 from = anchors[i];
                Vector3 to = anchors[i + 1];
                Vector3 delta = to - from;
                float length = delta.magnitude;
                Vector3 sideways = length > 0.0001f
                    ? new Vector3(-delta.y, delta.x, 0f) / length
                    : Vector3.zero;

                int pieces = Mathf.Max(1, Mathf.CeilToInt(length / _inGameParameter.ChainLightningSegmentLength));
                for (int step = 1; step < pieces; step++)
                {
                    float jitter = Random.Range(-1f, 1f) * _inGameParameter.ChainLightningJitter;
                    _zigzagBuffer.Add(Vector3.Lerp(from, to, (float)step / pieces) + sideways * jitter);
                }

                _zigzagBuffer.Add(to);
            }

            SetLinePositions(playback.Glow);
            SetLinePositions(playback.Core);
        }

        private void SetLinePositions(LineRenderer line)
        {
            line.positionCount = _zigzagBuffer.Count;
            for (int i = 0; i < _zigzagBuffer.Count; i++)
            {
                line.SetPosition(i, _zigzagBuffer[i]);
            }
        }

        private void ApplyLightningColor(LightningPlayback playback, float normalizedTime)
        {
            float fade = 1f - normalizedTime;

            var glowColor = _inGameParameter.ChainLightningColor;
            glowColor.a *= fade * 0.6f;
            playback.Glow.startColor = glowColor;
            playback.Glow.endColor = glowColor;

            var coreColor = new Color(1f, 1f, 1f, fade);
            playback.Core.startColor = coreColor;
            playback.Core.endColor = coreColor;
        }

        private Material GetLightningLineMaterial()
        {
            if (_assetHolder.LightningLineMaterial != null)
            {
                return _assetHolder.LightningLineMaterial;
            }

            if (_fallbackLineMaterial == null)
            {
                _fallbackLineMaterial = new Material(Shader.Find("Sprites/Default"));
            }

            return _fallbackLineMaterial;
        }

        private void UpdateLightningPlaybacks(float deltaTime)
        {
            for (int i = _lightningPlaybacks.Count - 1; i >= 0; i--)
            {
                var playback = _lightningPlaybacks[i];
                if (playback.Core == null)
                {
                    _lightningPlaybacks.RemoveAt(i);
                    continue;
                }

                playback.Elapsed += deltaTime;
                float normalizedTime = playback.Elapsed / _inGameParameter.ChainLightningLineSeconds;
                if (normalizedTime >= 1f)
                {
                    Object.Destroy(playback.Core.transform.parent.gameObject);
                    _lightningPlaybacks.RemoveAt(i);
                    continue;
                }

                playback.FlickerElapsed += deltaTime;
                if (playback.FlickerElapsed >= _inGameParameter.ChainLightningFlickerSeconds)
                {
                    playback.FlickerElapsed = 0f;
                    ApplyZigzag(playback);
                }

                ApplyLightningColor(playback, normalizedTime);
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
