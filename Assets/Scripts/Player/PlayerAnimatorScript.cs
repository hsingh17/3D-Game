using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimatorScript : MonoBehaviour
{
    public static readonly Dictionary<string, int> PlayerAnimationStates = new()
    {
        ["Idle"] = Animator.StringToHash("Idle"),
        ["Walk"] = Animator.StringToHash("Walk"),
        ["Sprint"] = Animator.StringToHash("Sprint"),
        ["JumpStart"] = Animator.StringToHash("JumpStart"),
        ["Falling"] = Animator.StringToHash("Falling"),
        ["Land"] = Animator.StringToHash("Land"),
    };

    private int currentState;
    private int newState;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        PlayerAnimationStates.TryGetValue("Idle", out currentState);
        newState = -1;
    }

    private void Update()
    {
        Debug.Log(currentState);
        if (currentState != newState && newState != -1)
        {
            animator.CrossFade(newState, 0.3f, 0);
            currentState = newState;
            newState = -1;
        }
    }

    public void SetState(string newStateName)
    {
        if (PlayerAnimationStates.TryGetValue(newStateName, out int tempState))
        {
            newState = currentState;
        }
    }
}
