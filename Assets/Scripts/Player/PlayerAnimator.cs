using UnityEngine;

[RequireComponent(typeof(GenericAnimator))]
public class PlayerAnimator : GenericAnimator
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
