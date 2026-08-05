using UnityEngine;

[CreateAssetMenu(fileName = "Entity", menuName = "Scriptable Objects/Entity")]
public class EntityScriptableObject : ScriptableObject
{
    public float jumpHeight;
    public float moveSpeed;
    public float gravity = -9.8f;
}
