using UnityEngine;
using UnityEngine.EventSystems;

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
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // This means the cursor is over a UI element like the inventory panel. In this case, we don't want clicks to register on non-UI objects behind the panel.
            return;
        }

        Inventory inventory = FindAnyObjectByType<Inventory>(); // Lazy but fine for now. But this will change. TODO
        if (inventory == null)
        {
            Debug.LogWarning("No Inventory found in scene!");
            return;
        }
        if (inventory.ItemInCursorSlot)
        {
            return;
        }
        Inventory.InventoryEntry itemEntry = new Inventory.InventoryEntry(itemData, 1);
        inventory.PutInventoryEntryInCursorSlot(itemEntry);
        Destroy(gameObject);
    }
}
