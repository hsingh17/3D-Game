using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Action<int> OnStart;
    public static Action<ItemInstance, int> OnAddItem;
    public static Action<int> OnRemoveItem;
    public static Action<InteractableItem, int> OnGetItem;

    [SerializeField]
    private ItemSpawner itemSpawner;

    [SerializeField]
    private int maxItems;

    [SerializeField]
    private List<ItemInstance> items;

    private int lastSelectedSlot = -1;

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

    public int AddItem(InteractableItem item)
    {
        int freeSlotIdx = lastSelectedSlot == -1 ? GetFirstFreeSlot() : lastSelectedSlot;
        if (freeSlotIdx != -1)
        {
            items[freeSlotIdx] = item.ItemInstance;
            OnAddItem?.Invoke(items[freeSlotIdx], freeSlotIdx);
            lastSelectedSlot = -1;
        }
        return freeSlotIdx;
    }

    public InteractableItem GetItem(int idx)
    {
        if (idx < 0 || idx >= items.Capacity)
        {
            return null;
        }

        InteractableItem item = items[idx] != null ? itemSpawner.SpawnItem(items[idx]) : null;

        // No item was retrieved when grabbing from this index.
        // Save the last selected slot for when player grabs item to put it into that slot
        lastSelectedSlot = item == null ? idx : -1;

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
}
