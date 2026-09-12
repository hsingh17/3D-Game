using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    public static readonly Dictionary<string, AnimationState> PlayerAnimationStates = new()
    {
        ["Idle"] = new AnimationState("Idle"),
        ["Walk"] = new AnimationState("Walk", 2f),
        ["Sprint"] = new AnimationState("Sprint"),
        ["JumpStart"] = new AnimationState("JumpStart"),
        ["Falling"] = new AnimationState("Falling"),
        ["Land"] = new AnimationState("Land"),
    };

    private Animator animator;
    private AnimationState currentState;
    private AnimationState newState;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        PlayerAnimationStates.TryGetValue("Idle", out currentState);
        newState = default;
    }

    private void Update()
    {
        if (!currentState.Equals(newState) && !newState.Equals(default))
        {
            animator.speed = newState.Speed;
            animator.CrossFade(newState.Hash, 0.3f, 0);
            currentState = newState;
            newState = default;
        }
    }

    public void SetState(string newStateName)
    {
        if (PlayerAnimationStates.TryGetValue(newStateName, out AnimationState tempState))
        {
            newState = tempState;
        }
    }
}
