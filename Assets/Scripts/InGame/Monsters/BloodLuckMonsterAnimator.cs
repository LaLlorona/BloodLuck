using UnityEngine;

namespace InGame.Monsters
{
    public class BloodLuckMonsterAnimator
    {
        private static readonly int AttackTrigger = Animator.StringToHash("Attack");

        private readonly Animator _animator;

        public BloodLuckMonsterAnimator(Animator animator)
        {
            _animator = animator;
        }

        public void PlayAttack()
        {
            if (_animator == null)
            {
                return;
            }

            _animator.SetTrigger(AttackTrigger);
        }
    }
}
