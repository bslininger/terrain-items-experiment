using UnityEngine;

public class WorldInventoryItem : MonoBehaviour
{
    [SerializeField] private Item itemData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        Inventory inventory = FindAnyObjectByType<Inventory>(); // Lazy but fine for now
        if (inventory != null)
        {
            inventory.AddItem(itemData, 1);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("No Inventory found in scene, kupo!");
        }

    }
}
