using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Action<int> OnStart;
    public static Action<Item, int> OnAddItem;
    public static Action<int> OnRemoveItem;
    public static Action<int, Item> OnGetItem;

    [SerializeField]
    private ItemSpawner itemSpawner;

    [SerializeField]
    private int maxItems;

    [SerializeField]
    private List<Item> items;

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
        int freeSlotIdx = lastSelectedSlot == -1 ? GetFirstFreeSlot() : lastSelectedSlot;
        if (freeSlotIdx != -1)
        {
            items[freeSlotIdx] = item;

            // If the item being picked up is not going into the hand,
            // destroy the item being picked up off ground
            if (!itemInHand)
            {
                // This not working cuz it destroys reference to item in list
                Destroy(item.gameObject);
            }

            OnAddItem?.Invoke(item, freeSlotIdx);
            lastSelectedSlot = -1;
        }
    }

    public Item GetItem(int idx)
    {
        if (idx < 0 || idx >= items.Capacity)
        {
            return null;
        }

        Item item = items[idx] ? items[idx] : null;
        // No item was retrieved when grabbing from here.
        // Save the last selected slot for when player grabs item to put it into that slot
        lastSelectedSlot = !item ? idx : -1;

        OnGetItem?.Invoke(idx, item);
        return item;
    }

    public void RemoveItem(Item item)
    {
        int idx = items.IndexOf(item);
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
            Item item = items[i];
            if (!item)
            {
                break;
            }
        }

        return i >= items.Capacity ? -1 : i;
    }
}
