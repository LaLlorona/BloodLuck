using System;
using InGame.System;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace InGame.UI
{
    public class BloodLuckGameResultViewComponent : MonoBehaviour
    {
        [SerializeField] private GameObject _victoryRoot;
        [SerializeField] private GameObject _defeatRoot;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _waveText;
        [SerializeField] private TMP_Text _storyText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private TMP_Text _restartButtonText;
        [SerializeField] private PlayableDirector _revealTimeline;

        public void Show(BloodLuckGameResultType resultType, int reachedWave, Action onRestart)
        {
            var isVictory = resultType == BloodLuckGameResultType.EscapedCasino;
            _victoryRoot.SetActive(isVictory);
            _defeatRoot.SetActive(!isVictory);
            _titleText.text = isVictory ? "VICTORY" : "DEFEAT";
            _waveText.text = $"Reached Wave {reachedWave}";
            _storyText.text = GetStory(resultType);

            _restartButtonText.text = isVictory ? "ESCAPE" : "RESTART";
            _restartButton.onClick.RemoveAllListeners();
            _restartButton.onClick.AddListener(() => onRestart?.Invoke());
            gameObject.SetActive(true);
            PlayRevealTimeline();
        }

        private void PlayRevealTimeline()
        {
            _revealTimeline.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            _revealTimeline.time = 0;
            _revealTimeline.Play();
        }

        private static string GetStory(BloodLuckGameResultType resultType)
        {
            switch (resultType)
            {
                case BloodLuckGameResultType.EscapedCasino:
                    return "You escaped the vampire's casino, now it's time to save another world.";
                case BloodLuckGameResultType.LostAtSlotMachine:
                    return "You reached the last slot machine, but you didn't have enough blood to win it.";
                case BloodLuckGameResultType.EatenByVampires:
                    return "You were eaten by vampires.";
                default:
                    throw new ArgumentOutOfRangeException(nameof(resultType), resultType, null);
            }
        }
    }
}
