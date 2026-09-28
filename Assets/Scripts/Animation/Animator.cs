using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.Animator))]
public class Animator : MonoBehaviour
{
    [SerializeField]
    private string idleStateName = "Idle";
    private UnityEngine.Animator animator;
    private AnimationState currentState;
    private AnimationState newState;
    private readonly Dictionary<string, AnimationState> animationStates = new() { };

    private void Awake()
    {
        animator = GetComponent<UnityEngine.Animator>();
        InitializeAnimationStatesMap();
        animationStates.TryGetValue(idleStateName, out currentState);
        newState = default;
    }

    private void Update()
    {
        if (!currentState.Equals(newState) && !newState.Equals(default))
        {
            animator.speed = newState.Speed;
            animator.CrossFade(newState.Hash, 0.1f, 0);
            currentState = newState;
            newState = default;
        }
    }

    public void SetState(string newStateName)
    {
        if (animationStates.TryGetValue(newStateName, out AnimationState tempState))
        {
            newState = tempState;
        }
    }

    private void InitializeAnimationStatesMap()
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            animationStates[clip.name] = new AnimationState(clip.name);
        }
    }
}
