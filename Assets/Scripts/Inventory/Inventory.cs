using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Action<int, int> OnStart;
    public static Action<ItemInstance, int> OnAddItem;
    public static Action<int> OnRemoveItem;
    public static Action<InteractableItem, int> OnGetItem;

    [SerializeField]
    private ItemSpawner itemSpawner;

    [SerializeField]
    private int maxHotbarSlots;

    [SerializeField]
    private int maxItems;

    [SerializeField]
    private List<ItemInstance> items;

    private int lastSelectedSlot = 0;

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
        OnStart?.Invoke(maxItems, maxHotbarSlots);
    }

    public int AddItem(InteractableItem item)
    {
        int freeSlotIdx = GetFirstFreeSlot();
        if (freeSlotIdx != -1)
        {
            items[freeSlotIdx] = item.ItemInstance;
            OnAddItem?.Invoke(items[freeSlotIdx], freeSlotIdx);
        }
        return freeSlotIdx;
    }

    public InteractableItem GetItemByDelta(float delta)
    {
        return GetItem(Mod(lastSelectedSlot + (int)delta, maxHotbarSlots));
    }

    public InteractableItem GetItem(int idx)
    {
        if (idx < 0 || idx >= items.Capacity)
        {
            return null;
        }

        InteractableItem item =
            items[idx] != null && items[idx].ItemData != null
                ? itemSpawner.SpawnItem(items[idx])
                : null;

        lastSelectedSlot = idx;
        OnGetItem?.Invoke(item, idx);
        return item;
    }

    public void RemoveItem(InteractableItem item)
    {
        int idx = items.IndexOf(item.ItemInstance);
        if (idx != -1)
        {
            OnRemoveItem?.Invoke(idx);
            items[idx] = null;
        }
    }

    private int GetFirstFreeSlot()
    {
        if (items[lastSelectedSlot] == null || items[lastSelectedSlot].ItemData == null)
        {
            return lastSelectedSlot;
        }

        int i;
        for (i = 0; i < items.Capacity; i++)
        {
            ItemInstance item = items[i];
            if (item == null || item.ItemData == null)
            {
                break;
            }
        }

        return i >= items.Capacity ? -1 : i;
    }

    private int Mod(int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }
}
