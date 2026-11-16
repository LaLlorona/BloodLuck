using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerAnimation
{
    public int Direction;
    [SerializeField] public Dictionary<PlayerAnimationState, string> AnimationStateNameDict;
}