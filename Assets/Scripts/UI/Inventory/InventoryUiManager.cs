using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PanelRenderer))]
public class InventoryUiManager : MonoBehaviour
{
    private List<VisualElement> hotbarSlots;

    private void OnEnable()
    {
        GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);
        Inventory.OnAddItem += AddItemSpriteToHotbar;
        Inventory.OnRemoveItem += RemoveItemSpriteToHotbar;
    }

    private void OnDisable()
    {
        GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);
        Inventory.OnAddItem -= AddItemSpriteToHotbar;
        Inventory.OnRemoveItem -= RemoveItemSpriteToHotbar;
    }

    private void OnUIReload(PanelRenderer pr, VisualElement root)
    {
        VisualElement hotbarContainer = root.Q<VisualElement>("inventory-hotbar-container");
        hotbarSlots = new();
        foreach (VisualElement child in hotbarContainer.Children())
        {
            hotbarSlots.Add(child);
        }
    }

    private void AddItemSpriteToHotbar(ItemData itemData, int idx)
    {
        if (idx >= 0 && idx < hotbarSlots.Count)
        {
            VisualElement slot = hotbarSlots[idx];
            Debug.Log($"adding sprite for {itemData.name} to hotbar slot {idx}");
            Image image = new() { sprite = itemData.itemSprite };
            slot.Add(image);
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
}
