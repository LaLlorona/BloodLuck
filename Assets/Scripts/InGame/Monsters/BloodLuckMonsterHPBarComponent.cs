using InGame.System;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Monsters
{
    public class BloodLuckMonsterHPBarComponent : MonoBehaviour
    {
        [Tooltip("Stretched child of the hp bar background. Its right anchor is scaled by the hp ratio.")]
        [SerializeField] private RectTransform _hpFill;
        [Tooltip("White bar drawn behind the fill. It stays at the old HP after damage and then catches up.")]
        [SerializeField] private Image _hpTrailingImage;

        private float _targetRatio = 1f;
        private float _trailRatio = 1f;
        private bool _isTrailWaiting;
        private float _trailHoldRemainingSeconds;
        private BloodLuckInGameParameter _parameter;

        public void SetHp(float currentHp, float maxHp, BloodLuckInGameParameter parameter)
        {
            _parameter = parameter;
            _targetRatio = maxHp > 0f ? Mathf.Clamp01(currentHp / maxHp) : 0f;
            _hpFill.anchorMax = new Vector2(_targetRatio, 1f);

            if (_targetRatio >= _trailRatio)
            {
                // Full HP (initial) or healed: nothing to show.
                _trailRatio = _targetRatio;
                _isTrailWaiting = false;
                ApplyTrail();
            }
        }

        // The trail waits for HpTrailHoldSeconds after the first damage and then drains toward the fill.
        // The wait is not extended by later hits. Drain speed grows with the gap, so big bursts drain fast.
        public void UpdateTrail(float deltaTime)
        {
            if (_parameter == null || _trailRatio <= _targetRatio)
            {
                return;
            }

            if (!_isTrailWaiting)
            {
                _isTrailWaiting = true;
                _trailHoldRemainingSeconds = _parameter.HpTrailHoldSeconds;
            }

            if (_trailHoldRemainingSeconds > 0f)
            {
                _trailHoldRemainingSeconds -= deltaTime;
                return;
            }

            float gap = _trailRatio - _targetRatio;
            float speed = Mathf.Max(_parameter.HpTrailMinSpeed, gap * _parameter.HpTrailSpeedPerGap);
            _trailRatio = Mathf.Max(_targetRatio, _trailRatio - speed * deltaTime);
            if (_trailRatio <= _targetRatio)
            {
                _isTrailWaiting = false;
            }

            ApplyTrail();
        }

        private void ApplyTrail()
        {
            if (_hpTrailingImage != null)
            {
                _hpTrailingImage.rectTransform.anchorMax = new Vector2(_trailRatio, 1f);
            }
        }
    }
}
