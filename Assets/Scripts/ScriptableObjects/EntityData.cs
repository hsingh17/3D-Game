using UnityEngine;

[CreateAssetMenu(fileName = "Entity", menuName = "Scriptable Objects/Entity")]
public class EntityData : ScriptableObject
{
    public float JumpForce;
    public float MoveSpeed;
    public float GroundDrag;
    public float AirDrag;
    public float GravityMultiplier = 1;
    public float SprintMultiplier = 1;
    public float SprintStaminaUsagePerSec = 20;
    public float MaxStamina = 100;
    public float StaminaRegenPerSec = 20;
}
