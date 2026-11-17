using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Action<int> OnStart;
    public static Action<ItemData, int> OnAddItem;
    public static Action<int> OnRemoveItem;
    public static Action<int, Item> OnGetItem;

    [SerializeField]
    private ItemSpawner itemSpawner;

    [SerializeField]
    private int maxItems;

    [SerializeField]
    private List<ItemData> items;

    private void Awake()
    {
        items = new(maxItems);
        // Preallocate list
        for (int i = 0; i < items.Capacity; i++)
        {
            items.Add(null);
        }
    }

    private void Start()
    {
        OnStart?.Invoke(maxItems);
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
        int freeSlotIdx = GetFirstFreeSlot();
        if (freeSlotIdx != -1)
        {
            items[freeSlotIdx] = item.itemData;

            // If the item being picked up is not going into the hand,
            // destroy the item being picked up off ground
            if (!itemInHand)
            {
                Destroy(item.gameObject);
            }

            OnAddItem?.Invoke(item.itemData, freeSlotIdx);
        }
    }

    public Item GetItem(int idx)
    {
        if (idx < 0 || idx >= items.Capacity)
        {
            return null;
        }

        Item item = items[idx] ? itemSpawner.SpawnItem(items[idx]) : null;
        OnGetItem?.Invoke(idx, item);
        return item;
    }

    public void RemoveItem(Item item)
    {
        int idx = items.IndexOf(item.itemData);
        if (idx != -1)
        {
            ItemData itemData = items[idx];
            OnRemoveItem?.Invoke(idx);
            items[idx] = null;
        }
    }

    private int GetFirstFreeSlot()
    {
        int i;
        for (i = 0; i < items.Capacity; i++)
        {
            ItemData item = items[i];
            if (!item)
            {
                break;
            }
        }

        return i >= items.Capacity ? -1 : i;
    }
}
