using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class ItemData : ScriptableObject
{
    public string ItemName;
    public GameObject ItemPrefab;
    public Sprite ItemSprite;
    public Vector3 InHandLocalPosition;
    public Vector3 InHandLocalRotation;
}
