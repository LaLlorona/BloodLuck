using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace InGame.UI
{
    public class BloodLuckGameResultViewComponent : MonoBehaviour
    {
        [SerializeField] private GameObject _victoryRoot;
        [SerializeField] private GameObject _defeatRoot;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _waveText;
        [FormerlySerializedAs("_pullsText")]
        [FormerlySerializedAs("_maxHpText")]
        [SerializeField] private TMP_Text _hpText;
        [SerializeField] private Button _restartButton;

        public void Show(bool isVictory, int reachedWave, float finalHp, Action onRestart)
        {
            _victoryRoot.SetActive(isVictory);
            _defeatRoot.SetActive(!isVictory);
            _titleText.text = isVictory ? "VICTORY" : "DEFEAT";
            _waveText.text = $"WAVE {reachedWave}";
            _hpText.text = $"HP {Mathf.FloorToInt(finalHp)}";

            _restartButton.onClick.RemoveAllListeners();
            _restartButton.onClick.AddListener(() => onRestart?.Invoke());
            gameObject.SetActive(true);
        }
    }
}
