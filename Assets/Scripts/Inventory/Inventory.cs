using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Action<ItemData, int> OnAddItem;
    public static Action<int> OnRemoveItem;

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

    private void OnEnable()
    {
        Hand.OnGrabItem += AddItem;
        Hand.OnThrowReleaseEvent += RemoveItem;
    }

    private void OnDisable()
    {
        Hand.OnGrabItem -= AddItem;
        Hand.OnThrowReleaseEvent -= RemoveItem;
    }

    public void AddItem(Item item, bool itemInHand)
    {
        if (items.Count < maxItems)
        {
            items.Add(item.itemData);

            if (!itemInHand)
            {
                Destroy(item.gameObject);
            }

            OnAddItem?.Invoke(item.itemData, items.Count - 1);
        }
    }

    public Item GetItem(int idx)
    {
        return idx < 0 || idx >= items.Count ? null : itemSpawner.SpawnItem(items[idx]);
    }

    public void RemoveItem(Item item)
    {
        int idx = items.IndexOf(item.itemData);
        if (idx != -1)
        {
            ItemData itemData = items[idx];
            OnRemoveItem?.Invoke(idx);
            items.RemoveAt(idx);
        }
    }
}
