using UnityEngine;

namespace InGame.Monsters
{
    public class BloodLuckMonsterComponent : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _hitFlashColor = Color.white;
        [SerializeField, Min(0.01f)] private float _hitFlashSeconds = 0.15f;

        private Color _baseColor;
        private float _flashElapsed;
        private bool _isFlashing;
        private BloodLuckMonster _bloodLuckMonster;

        public void Init(BloodLuckMonster bloodLuckMonster)
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            _bloodLuckMonster = bloodLuckMonster;
            _baseColor = _spriteRenderer.color;
            _isFlashing = false;
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
