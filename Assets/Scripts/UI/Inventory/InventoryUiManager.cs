using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class InventoryUiManager : MonoBehaviour
{
    private static readonly string inventoryHotbarContainerClassName = "inventory-hotbar-container";
    private static readonly string inventoryHotbarSlotClassName = "inventory-hotbar-slot";
    private static readonly string inventoryHotbarUnselectedSlotClassName =
        "inventory-hotbar-slot-unselected";
    private static readonly string inventoryHotbarSelectedSlotClassName =
        "inventory-hotbar-slot-selected";
    private static readonly string inventoryHotbarSlotNumberClassName =
        "inventory-hotbar-slot-number";

    private static readonly string inventoryHotbarSlotSpriteClassName =
        "inventory-hotbar-slot-sprite";

    [SerializeField]
    private int maxHotbarSlots = 10;
    private int maxItems;
    private int curSelectedHotbarSlot = 0;
    private List<VisualElement> hotbarSlots;

    private void OnEnable()
    {
        GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);
        Inventory.OnStart += SetMaxItems;
        Inventory.OnAddItem += AddItemSpriteToHotbar;
        Inventory.OnRemoveItem += RemoveItemSpriteToHotbar;
        Inventory.OnGetItem += HighlightInventorySlot;
    }

    private void OnDisable()
    {
        GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);
        Inventory.OnStart -= SetMaxItems;
        Inventory.OnAddItem -= AddItemSpriteToHotbar;
        Inventory.OnRemoveItem -= RemoveItemSpriteToHotbar;
        Inventory.OnGetItem -= HighlightInventorySlot;
    }

    private void OnUIReload(PanelRenderer pr, VisualElement root)
    {
        DrawHotbarSlots(root);
        HighlightInventorySlot(curSelectedHotbarSlot, null);
    }

    private void DrawHotbarSlots(VisualElement root)
    {
        VisualElement hotbarContainer = root.Q<VisualElement>(inventoryHotbarContainerClassName);
        hotbarSlots = new();

        for (int i = 0; i < maxHotbarSlots; i++)
        {
            int slotNumber = (i + 1) % maxHotbarSlots;
            VisualElement inventoryHotbarSlot = new();
            inventoryHotbarSlot.AddToClassList(inventoryHotbarSlotClassName);
            inventoryHotbarSlot.AddToClassList(inventoryHotbarUnselectedSlotClassName);
            inventoryHotbarSlot.name = $"inventory-hotbar-slot-{slotNumber}";

            Label slotLabel = new(slotNumber.ToString());
            slotLabel.AddToClassList(inventoryHotbarSlotNumberClassName);
            inventoryHotbarSlot.Add(slotLabel);

            hotbarContainer.Add(inventoryHotbarSlot);
            hotbarSlots.Add(inventoryHotbarSlot);
        }
    }

    private void SetMaxItems(int maxItems)
    {
        this.maxItems = maxItems;
    }

    private void AddItemSpriteToHotbar(Item item, int idx)
    {
        if (idx >= 0 && idx < hotbarSlots.Count)
        {
            VisualElement slot = hotbarSlots[idx];
            Image image = new() { sprite = item.itemData.itemSprite };
            slot.Add(image);
            image.SendToBack();
            image.AddToClassList(inventoryHotbarSlotSpriteClassName);
        }
    }

    private void RemoveItemSpriteToHotbar(int idx)
    {
        if (idx >= 0 && idx < hotbarSlots.Count)
        {
            VisualElement slot = hotbarSlots[idx];
            Image image = slot.Q<Image>();
            slot.Remove(image);
        }
    }

    private void HighlightInventorySlot(int idx, Item item)
    {
        if (idx >= 0 && idx < maxHotbarSlots)
        {
            // Remove styling on the previously selected hotbar slot
            if (curSelectedHotbarSlot >= 0 && curSelectedHotbarSlot < maxHotbarSlots)
            {
                hotbarSlots[curSelectedHotbarSlot]
                    .RemoveFromClassList(inventoryHotbarSelectedSlotClassName);
                hotbarSlots[curSelectedHotbarSlot]
                    .AddToClassList(inventoryHotbarUnselectedSlotClassName);
            }

            hotbarSlots[idx].RemoveFromClassList(inventoryHotbarUnselectedSlotClassName);
            hotbarSlots[idx].AddToClassList(inventoryHotbarSelectedSlotClassName);
            curSelectedHotbarSlot = idx;
        }
    }
}
