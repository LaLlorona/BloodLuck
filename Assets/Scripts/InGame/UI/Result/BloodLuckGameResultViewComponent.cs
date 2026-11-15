using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI
{
    public class BloodLuckGameResultViewComponent : MonoBehaviour
    {
        [SerializeField] private GameObject _victoryRoot;
        [SerializeField] private GameObject _defeatRoot;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _waveText;
        [SerializeField] private TMP_Text _pullsText;
        [SerializeField] private Button _restartButton;

        public void Show(bool isVictory, int reachedWave, int remainingPulls, int maxPulls, Action onRestart)
        {
            _victoryRoot.SetActive(isVictory);
            _defeatRoot.SetActive(!isVictory);
            _titleText.text = isVictory ? "VICTORY" : "DEFEAT";
            _waveText.text = $"WAVE {reachedWave}";
            _pullsText.text = $"{remainingPulls}/{maxPulls}";

            _restartButton.onClick.RemoveAllListeners();
            _restartButton.onClick.AddListener(() => onRestart?.Invoke());
            gameObject.SetActive(true);
        }
    }
}
