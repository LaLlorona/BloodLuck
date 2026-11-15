using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI
{
    public class SlotMachineViewComponent : MonoBehaviour
    {
        [SerializeField] private Sprite[] _symbols;
        [SerializeField] private SlotReelViewComponent[] _reels;
        [SerializeField] private Button _spinButton;
        [SerializeField] private TMP_Text _pullsText;
        [SerializeField] private TMP_Text _resultText;

        [Header("Spin")]
        [SerializeField, Min(0.05f)] private float _firstReelSpinSeconds = 0.8f;
        [SerializeField, Min(0f)] private float _reelStopIntervalSeconds = 0.4f;
        [SerializeField, Min(0.01f)] private float _symbolSwapSeconds = 0.06f;

        public int SymbolCount => _symbols.Length;
        public int ReelCount => _reels.Length;

        public void Init()
        {
            foreach (var reel in _reels)
            {
                reel.Init(_symbols);
            }

            _resultText.text = string.Empty;
        }

        public void SetPulls(int currentPulls, int maxPulls)
        {
            _pullsText.text = $"{currentPulls}/{maxPulls}";
        }

        public void SetSpinInteractable(bool interactable)
        {
            _spinButton.interactable = interactable;
        }

        public void SetResultText(string text)
        {
            _resultText.text = text;
        }

        public UniTask WaitSpinClickAsync(CancellationToken ct)
        {
            return _spinButton.OnClickAsync(ct);
        }

        public async UniTask PlaySpinAsync(int[] resultIndices, CancellationToken ct)
        {
            SetSpinInteractable(false);
            _resultText.text = string.Empty;

            var tasks = new UniTask[_reels.Length];
            for (int i = 0; i < _reels.Length; i++)
            {
                float duration = _firstReelSpinSeconds + _reelStopIntervalSeconds * i;
                tasks[i] = _reels[i].SpinAsync(resultIndices[i], duration, _symbolSwapSeconds, ct);
            }

            await UniTask.WhenAll(tasks);
        }
    }
}
