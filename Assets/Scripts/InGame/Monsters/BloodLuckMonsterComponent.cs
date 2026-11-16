using InGame.System;
using UnityEngine;

namespace InGame.Monsters
{
    public class BloodLuckMonsterComponent : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private BloodLuckMonsterHPBarComponent _hpBar;
        [SerializeField] private Color _hitFlashColor = Color.white;
        [SerializeField, Min(0.01f)] private float _hitFlashSeconds = 0.15f;

        private Color _baseColor;
        private float _flashElapsed;
        private bool _isFlashing;
        private BloodLuckMonster _bloodLuckMonster;
        private BloodLuckInGameParameter _parameter;

        public BloodLuckMonster Monster => _bloodLuckMonster;

        public void Init(BloodLuckMonster bloodLuckMonster, BloodLuckInGameParameter parameter)
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            _bloodLuckMonster = bloodLuckMonster;
            _parameter = parameter;
            _baseColor = _spriteRenderer.color;
            _isFlashing = false;
        }

        public void SetHp(float currentHp, float maxHp)
        {
            _hpBar.SetHp(currentHp, maxHp, _parameter);
        }

        public void OnMonsterDamage(float damage)
        {
            _bloodLuckMonster.OnMonsterDamage(damage);
        }
        
        public void PlayHitFlash()
        {
            _flashElapsed = 0f;
            _isFlashing = true;
            _spriteRenderer.color = _hitFlashColor;
        }

        public void CustomUpdate(float deltaTime)
        {
            _hpBar.UpdateTrail(deltaTime);

            if (!_isFlashing)
            {
                return;
            }

            _flashElapsed += deltaTime;
            float t = Mathf.Clamp01(_flashElapsed / _hitFlashSeconds);
            _spriteRenderer.color = Color.Lerp(_hitFlashColor, _baseColor, t);

            if (t >= 1f)
            {
                _isFlashing = false;
            }
        }
    }
}
