using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class ItemData : ScriptableObject
{
    public string ItemName;
    public GameObject ItemPrefab;
    public Sprite ItemSprite;
}
