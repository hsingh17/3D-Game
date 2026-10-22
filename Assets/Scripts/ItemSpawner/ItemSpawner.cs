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

    public void SpawnItem(ItemData itemData, Transform spawnPoint = null)
    {
        GameObject spawnedItem = Instantiate(
            itemData.itemPrefab,
            spawnPoint != null ? spawnPoint : transform
        );
        spawnedItem.transform.SetParent(itemsParentGameObject.transform);
        spawnedItem.TryGetComponent(out Item item);
        item.itemsParentGameObject = itemsParentGameObject;
    }
}
