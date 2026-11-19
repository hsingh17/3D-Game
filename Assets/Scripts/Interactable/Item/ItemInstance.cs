using System;
using UnityEngine;

[Serializable]
public class ItemInstance
{
    public string ItemId = Guid.NewGuid().ToString();
    public GameObject ItemsParentGameObject;
    public ItemData ItemData;

    public ItemInstance(ItemInstance itemInstance)
    {
        ItemsParentGameObject = itemInstance.ItemsParentGameObject;
        ItemData = itemInstance.ItemData;
        ItemId = itemInstance.ItemId;
    }

    public ItemInstance(ItemData itemData, GameObject itemsParentGameObject)
    {
        ItemData = itemData;
        ItemsParentGameObject = itemsParentGameObject;
    }

    public override bool Equals(object obj)
    {
        return obj != null
            && obj.GetType() == GetType()
            && obj is ItemInstance item
            && item.ItemId == ItemId;
    }

    public override int GetHashCode()
    {
        return ItemId.GetHashCode();
    }
}
