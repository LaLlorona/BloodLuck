using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI
{
    public class SlotReelViewComponent : MonoBehaviour
    {
        private const int MiddleRowIndex = 1;

        [SerializeField] private Image[] _rows;

        private Sprite[] _symbols;

        public void Init(Sprite[] symbols)
        {
            _symbols = symbols;
            SetRows(RandomIndex(), RandomIndex(), RandomIndex());
        }

        public async UniTask SpinAsync(int resultIndex, float duration, float tickSeconds, CancellationToken ct)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                SetRows(RandomIndex(), RandomIndex(), RandomIndex());
                await UniTask.Delay(TimeSpan.FromSeconds(tickSeconds), DelayType.UnscaledDeltaTime, cancellationToken: ct);
                elapsed += tickSeconds;
            }

            SetRows(RandomIndex(), resultIndex, RandomIndex());
        }

        private void SetRows(int top, int middle, int bottom)
        {
            _rows[0].sprite = _symbols[top];
            _rows[MiddleRowIndex].sprite = _symbols[middle];
            _rows[2].sprite = _symbols[bottom];
        }

        private int RandomIndex() => UnityEngine.Random.Range(0, _symbols.Length);
    }
}
