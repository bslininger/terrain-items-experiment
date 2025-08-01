using UnityEngine;
using UnityEngine.EventSystems;

public class WorldInventoryItem : MonoBehaviour
{
    [SerializeField] private Item itemData;
    IInputLockProvider inputLockProvider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputLockProvider = UIManager.Instance;
    }

    private void OnMouseDown()
    {
        if (inputLockProvider.InputLocked(UIInputLock.InventoryInteraction))
        {
            return;
        }

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
        UIManager.Instance.ActivateInventoryPanel();
        inventory.PutInventoryEntryInCursorSlot(itemEntry);
        Destroy(gameObject);
    }
}
