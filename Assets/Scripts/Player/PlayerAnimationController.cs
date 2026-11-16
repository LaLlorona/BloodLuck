using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public Animator Animator;
    [SerializeField] private List<PlayerAnimation> playerAnimations;
    public int CurrDirection;
    public PlayerAnimationState CurrAnimState;

    void Awake()
    {
        Animator = GetComponent<Animator>();
    }

    public void PlayAnimation(int facingIndex, PlayerAnimationState animState, float playbackSpeed = 1f)
    {
        PlayerAnimation anim = playerAnimations.FirstOrDefault(a => a.Direction == facingIndex);
        if (anim == null)
        {
            return;
        }
        string animName = anim.AnimationStateNameDict.GetValueOrDefault(animState, "");
        if (animName == "")
        {
            Debug.LogWarning($"Missing player animation for facingIndex ({facingIndex}) and animState ({animState})");
            return;
        }

        CurrDirection = facingIndex;
        CurrAnimState = animState;

        Animator.speed = playbackSpeed;
        Animator.Play(animName);
    }

    public void UpdateDirection(int facingIndex)
    {
        PlayAnimation(facingIndex, CurrAnimState);
    }

    public void UpdateAnimState(PlayerAnimationState animState, float playbackSpeed = 1f)
    {
        PlayAnimation(CurrDirection, animState, playbackSpeed);
    }
}
