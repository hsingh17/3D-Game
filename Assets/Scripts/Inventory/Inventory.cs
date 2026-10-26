using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    private ItemSpawner itemSpawner;

    [SerializeField]
    private int maxItems;

    [SerializeField]
    private List<ItemData> items;

    private void Awake()
    {
        items = new(maxItems);
    }

    public void PutItemInInventory(Item item)
    {
        if (items.Count >= maxItems)
        {
            return;
        }

        items.Add(item.itemData);
        Destroy(item.gameObject);
    }

    public Item PullItemFromInventory(int idx)
    {
        return idx < 0 || idx >= items.Count ? null : itemSpawner.SpawnItem(items[idx]);
    }
}
