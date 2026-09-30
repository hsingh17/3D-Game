using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class GenericAnimator : MonoBehaviour
{
    [SerializeField]
    private string idleStateName = "Idle";
    private Animator animator;
    private AnimationState currentState;
    private AnimationState newState;
    private readonly Dictionary<string, AnimationState> animationStates = new() { };

    private void Awake()
    {
        animator = GetComponent<Animator>();
        InitializeAnimationStatesMap();
        animationStates.TryGetValue(idleStateName, out currentState);
        newState = default;
    }

    public virtual void Update()
    {
        if (!currentState.Equals(newState) && !newState.Equals(default))
        {
            animator.speed = newState.Speed;
            animator.CrossFade(newState.Hash, 0.1f, 0);
            currentState = newState;
            newState = default;
        }
    }

    protected void SetState(string newStateName)
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
