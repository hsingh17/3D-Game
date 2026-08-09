using UnityEngine;

[CreateAssetMenu(fileName = "Entity", menuName = "Scriptable Objects/Entity")]
public class EntityScriptableObject : ScriptableObject
{
    public float jumpForce;
    public float moveSpeed;
    public float groundDrag;
    public float airDrag;
    public float gravityMultiplier = 1;
}
