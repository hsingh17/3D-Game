using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public static ItemSpawner instance;

    [SerializeField]
    private GameObject itemsParentGameObject;

    [SerializeField]
    private ItemData itemToSpawn;

    private void Awake()
    {
        instance = instance != null ? instance : this;
    }

    private void Update()
    {
        if (itemToSpawn)
        {
            SpawnItem(itemToSpawn);
            itemToSpawn = null;
        }
    }

    public InteractableItem SpawnItem(ItemInstance itemInstance, Transform spawnPoint = null)
    {
        InteractableItem item = SpawnItem(itemInstance.ItemData, spawnPoint);
        item.ItemInstance = itemInstance;
        return item;
    }

    public InteractableItem SpawnItem(ItemData itemData, Transform spawnPoint = null)
    {
        GameObject spawnedItem = Instantiate(
            itemData.ItemPrefab,
            spawnPoint != null ? spawnPoint : transform
        );

        spawnedItem.TryGetComponent(out InteractableItem item);
        item.PostInstantiate(itemData, itemsParentGameObject);

        return item;
    }
}
