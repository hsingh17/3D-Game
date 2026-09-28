using UnityEngine;

[RequireComponent(typeof(UnityEngine.Animator))]
public class PlayerAnimator : Animator
{
    [SerializeField]
    private PlayerController playerController;

    public override void Update()
    {
        UpdatePlayerState();
        base.Update();
    }

    private void UpdatePlayerState()
    {
        if (!playerController.IsGrounded)
        {
            SetState("Falling");
        }
        else if (playerController.IsSprinting())
        {
            SetState("Sprint");
        }
        else if (playerController.IsWalkingForward())
        {
            SetState("WalkForward");
        }
        else if (playerController.IsWalkingBackward())
        {
            SetState("WalkBackward");
        }
        else
        {
            SetState("Idle");
        }
    }
}
