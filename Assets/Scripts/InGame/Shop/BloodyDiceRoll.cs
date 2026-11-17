using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace BloodLuck
{
    /// <summary>
    /// Visual-only dice sequence. BloodLuckShop selects and owns the reward before calling Roll.
    /// Set face rotations in the Inspector after checking the imported D6 through DiceCamera.
    /// </summary>
    public class BloodyDiceRoll : MonoBehaviour
    {
        [Header("Existing Dice Setup")]
        [SerializeField] private GameObject rollDisplay;
        [SerializeField] private Transform dice;
        [SerializeField] private TMP_Text rewardLabel;

        [Header("Face Orientations (one per visible D6 symbol)")]
        [Tooltip("Euler rotations that present each physical dice face to DiceCamera. Index is selected consistently from the reward asset name.")]
        [SerializeField] private Vector3[] faceRotations =
        {
            new Vector3(0, 0, 0), new Vector3(0, 90, 0), new Vector3(0, 180, 0),
            new Vector3(0, 270, 0), new Vector3(90, 0, 0), new Vector3(270, 0, 0)
        };
        [SerializeField, Min(0.1f)] private float rollDuration = 1.35f;
        [SerializeField, Min(0f)] private float revealDuration = 1.4f;
        [SerializeField, Min(0f)] private float bounceHeight = 0.12f;

        [Header("Hooks")]
        [SerializeField] private UnityEvent onRollStarted;
        [SerializeField] private UnityEvent onDiceLanded;
        [SerializeField] private UnityEvent onRewardRevealed;

        public bool IsRolling { get; private set; }

        private void Awake()
        {
            if (rollDisplay != null) rollDisplay.SetActive(false);
        }

        public void ShowDice()
        {
            if (rollDisplay != null) rollDisplay.SetActive(true);
            if (rewardLabel != null) rewardLabel.text = string.Empty;
        }

        public void HideDice()
        {
            if (rollDisplay != null) rollDisplay.SetActive(false);
        }

        public void Roll(InfusionDefinition reward, Action completed, bool hideDisplayAfterReveal = true)
        {
            if (IsRolling || reward == null)
            {
                completed?.Invoke();
                return;
            }
            StartCoroutine(RollRoutine(reward, completed, hideDisplayAfterReveal));
        }

        private IEnumerator RollRoutine(InfusionDefinition reward, Action completed, bool hideDisplayAfterReveal)
        {
            IsRolling = true;
            if (rollDisplay != null) rollDisplay.SetActive(true);
            if (dice == null)
            {
                yield return new WaitForSecondsRealtime(0.1f);
                Finish(completed);
                yield break;
            }

            int face = FaceFor(reward);
            Quaternion startRotation = dice.localRotation;
            Quaternion finalRotation = Quaternion.Euler(faceRotations[face]);
            Vector3 startPosition = dice.localPosition;
            AudioManager.Instance?.PlayDiceRoll();
            onRollStarted?.Invoke();

            float elapsed = 0f;
            while (elapsed < rollDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / rollDuration);
                // The extra turns diminish to zero so the last frame is exactly the chosen face.
                float turns = (1f - t) * (1f - t) * 4f;
                Quaternion tumble = Quaternion.Euler(turns * 360f, turns * 510f, turns * 290f);
                dice.localRotation = Quaternion.Slerp(startRotation, finalRotation, t) * tumble;
                yield return null;
            }

            dice.localRotation = finalRotation;
            onDiceLanded?.Invoke();
            float bounceElapsed = 0f;
            const float bounceDuration = 0.25f;
            while (bounceElapsed < bounceDuration)
            {
                bounceElapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(bounceElapsed / bounceDuration);
                dice.localPosition = startPosition + Vector3.up * (Mathf.Sin(t * Mathf.PI) * bounceHeight);
                yield return null;
            }
            dice.localPosition = startPosition;

            if (rewardLabel != null) rewardLabel.text = reward.displayName;
            onRewardRevealed?.Invoke();
            yield return new WaitForSecondsRealtime(revealDuration);
            if (hideDisplayAfterReveal && rollDisplay != null) rollDisplay.SetActive(false);
            Finish(completed);
        }

        private void Finish(Action completed)
        {
            IsRolling = false;
            completed?.Invoke();
        }

        // This stable, asset-name based modulo mapping supports any number of mythics without a reward table.
        private int FaceFor(InfusionDefinition reward)
        {
            if (faceRotations == null || faceRotations.Length == 0) return 0;
            int hash = 17;
            string key = reward.name + reward.displayName;
            for (int i = 0; i < key.Length; i++) hash = hash * 31 + key[i];
            return (hash & int.MaxValue) % faceRotations.Length;
        }
    }
}
