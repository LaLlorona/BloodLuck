using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI
{
    public class SlotReelViewComponent : MonoBehaviour
    {
        private const float FullTurnRadians = Mathf.PI * 2f;

        [SerializeField] private Image[] _rows;
        [SerializeField, Min(3)] private int _faceCount = 10;
        [SerializeField, Min(1f)] private float _cameraDistance = 900f;
        [SerializeField, Min(0.5f)] private float _turnsPerSecond = 2.5f;

        private SlotDrumFaceMeshEffect[] _faces;
        private int[] _faceSymbolIndices;
        private float _faceStepRadians;
        private float _drumAngleRadians;

        public void Init(Sprite[] symbols)
        {
            _faceStepRadians = FullTurnRadians / _faceCount;
            PrepareMask();
            BuildFaces(symbols);
            _drumAngleRadians = -_faceStepRadians * Random.Range(0, _faceCount);
            ApplyDrumAngle();
        }

        public async UniTask SpinAsync(int resultSymbolIndex, float duration, CancellationToken ct)
        {
            float startAngle = _drumAngleRadians;
            float endAngle = -_faceStepRadians * PickFaceShowingSymbol(resultSymbolIndex);
            float totalRotation = ComputeTotalRotation(startAngle, endAngle, duration);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
                elapsed += Time.unscaledDeltaTime;
                _drumAngleRadians = startAngle - totalRotation * EaseOutCubic(Mathf.Clamp01(elapsed / duration));
                ApplyDrumAngle();
            }

            _drumAngleRadians = endAngle;
            ApplyDrumAngle();
        }

        private void PrepareMask()
        {
            if (!TryGetComponent<RectMask2D>(out _))
            {
                gameObject.AddComponent<RectMask2D>();
            }
        }

        private void BuildFaces(Sprite[] symbols)
        {
            var projection = new SlotDrumProjection(_rows[0].rectTransform.rect.height, _faceCount, _cameraDistance);
            int symbolOffset = Random.Range(0, symbols.Length);

            _faces = new SlotDrumFaceMeshEffect[_faceCount];
            _faceSymbolIndices = new int[_faceCount];
            for (int i = 0; i < _faceCount; i++)
            {
                Image image = i < _rows.Length ? _rows[i] : Instantiate(_rows[0], _rows[0].transform.parent);
                image.rectTransform.anchoredPosition = Vector2.zero;
                image.preserveAspect = true;

                _faceSymbolIndices[i] = (i + symbolOffset) % symbols.Length;
                image.sprite = symbols[_faceSymbolIndices[i]];

                if (!image.TryGetComponent(out SlotDrumFaceMeshEffect effect))
                {
                    effect = image.gameObject.AddComponent<SlotDrumFaceMeshEffect>();
                }

                effect.Configure(projection);
                _faces[i] = effect;
            }
        }

        private int PickFaceShowingSymbol(int symbolIndex)
        {
            var candidates = new List<int>();
            for (int i = 0; i < _faceSymbolIndices.Length; i++)
            {
                if (_faceSymbolIndices[i] == symbolIndex)
                {
                    candidates.Add(i);
                }
            }

            return candidates[Random.Range(0, candidates.Count)];
        }

        private float ComputeTotalRotation(float startAngle, float endAngle, float duration)
        {
            float remainder = Mathf.Repeat(startAngle - endAngle, FullTurnRadians);
            float desiredRotation = _turnsPerSecond * FullTurnRadians * duration;
            int extraTurns = Mathf.Max(1, Mathf.RoundToInt((desiredRotation - remainder) / FullTurnRadians));
            return remainder + extraTurns * FullTurnRadians;
        }

        private void ApplyDrumAngle()
        {
            for (int i = 0; i < _faces.Length; i++)
            {
                _faces[i].SetFaceAngle(_drumAngleRadians + _faceStepRadians * i);
            }
        }

        private static float EaseOutCubic(float t)
        {
            float inverse = 1f - t;
            return 1f - inverse * inverse * inverse;
        }
    }
}
