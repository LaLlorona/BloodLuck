using InGame.System;
using UnityEngine;

namespace InGame.Monsters
{
    public class BloodLuckMonsterComponent : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator _animator;
        [SerializeField] private BloodLuckMonsterHPBarComponent _hpBar;
        [SerializeField] private bool _spriteFacesRight = true;
        [SerializeField, Min(0f)] private float _facingDeadZone = 0.05f;
        [SerializeField] private Color _hitFlashColor = Color.white;
        [SerializeField, Min(0.01f)] private float _hitFlashSeconds = 0.15f;

        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
        private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");

        private MaterialPropertyBlock _propertyBlock;
        private float _flashElapsed;
        private bool _isFlashing;
        private BloodLuckMonster _bloodLuckMonster;
        private BloodLuckInGameParameter _parameter;

        public BloodLuckMonster Monster => _bloodLuckMonster;
        public BloodLuckMonsterAnimator MonsterAnimator { get; private set; }

        public void Init(BloodLuckMonster bloodLuckMonster, BloodLuckInGameParameter parameter)
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            MonsterAnimator = new BloodLuckMonsterAnimator(_animator);
            _bloodLuckMonster = bloodLuckMonster;
            _parameter = parameter;
            _propertyBlock = new MaterialPropertyBlock();
            _isFlashing = false;
            ApplyFlash(0f);
        }

        public void FaceHorizontal(float directionX)
        {
            if (Mathf.Abs(directionX) <= _facingDeadZone)
            {
                return;
            }

            bool facesRight = directionX > 0f;
            _spriteRenderer.flipX = facesRight != _spriteFacesRight;
        }

        public void SetHp(float currentHp, float maxHp)
        {
            _hpBar.SetHp(currentHp, maxHp, _parameter);
        }

        public void OnMonsterDamage(float damage, BloodLuckDamageSource source)
        {
            _bloodLuckMonster.OnMonsterDamage(damage, source);
        }
        
        public void PlayHitFlash()
        {
            _flashElapsed = 0f;
            _isFlashing = true;
            ApplyFlash(1f);
        }

        private void ApplyFlash(float amount)
        {
            _spriteRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(FlashColorId, _hitFlashColor);
            _propertyBlock.SetFloat(FlashAmountId, amount);
            _spriteRenderer.SetPropertyBlock(_propertyBlock);
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
            ApplyFlash(1f - t);

            if (t >= 1f)
            {
                _isFlashing = false;
            }
        }
    }
}
