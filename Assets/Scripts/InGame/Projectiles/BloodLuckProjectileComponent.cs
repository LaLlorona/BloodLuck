using UnityEngine;

namespace InGame.Projectiles
{
    public class BloodLuckProjectileComponent : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private bool _spriteFacesRight = true;

        public void FaceDirection(Vector2 direction)
        {
            if (Mathf.Approximately(direction.x, 0f))
            {
                return;
            }

            bool facesRight = direction.x > 0f;
            _spriteRenderer.flipX = facesRight != _spriteFacesRight;
        }
    }
}
