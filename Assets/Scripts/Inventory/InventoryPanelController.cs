using UnityEngine;

public class InventoryPanelController : MonoBehaviour
{
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private InventoryController inventoryController;
    [SerializeField] private GameObject slotPrefab;
    private GameObject[] inventorySlots;


    private void OnEnable()
    {
        EventManager.InventoryUpdateEvent += UpdateDirtySlots;
        PopulatePanelFromInventory();
    }

    private void OnDisable()
    {
        EventManager.InventoryUpdateEvent -= UpdateDirtySlots;
        DestroyAllSlots();
    }

    public void ActivateInventoryPanel()
    {
        gameObject.SetActive(true);
    }

    public void DeactivateInventoryPanel()
    {
        gameObject.SetActive(false);
    }

    public void ToggleInventoryPanel()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

     void PopulatePanelFromInventory()
     {
        // First, generate all slots
        inventorySlots = new GameObject[playerInventory.InventorySize];
        for (int index = 0; index < playerInventory.InventorySize; ++index)
        {
            inventorySlots[index] = Instantiate(slotPrefab, transform);
            inventorySlots[index].name = $"Inventory Slot {index}";
            InventorySlotUIController slotUIController = inventorySlots[index].transform.GetComponent<InventorySlotUIController>();
            slotUIController.InputLockProvider = UIManager.Instance;
            inventoryController.RegisterUIInventorySlot(slotUIController, index);
        }

        // Now, put inventory items into their proper slots in the inventory panel, based on their index locations in the inventory itself
        for (int index = 0; index < playerInventory.InventorySize; ++index)
        {
            InventorySlotUIController slotUIController = inventorySlots[index].transform.GetComponent<InventorySlotUIController>();
            slotUIController.SetSlot(playerInventory.GetSlotDisplayInformation(index));
        }
    }

    void UpdateDirtySlots(int[] indicesToUpdate)
    {
        foreach (int indexToUpdate in indicesToUpdate)
        {
            if (indexToUpdate >= 0)  // index -1 (Inventory.CursorSlotIndex) is used for the cursor inventory slot
                UpdateSlot(indexToUpdate);
        }
    }

    void UpdateSlot(int index)
    {
        InventorySlotUIController slotUIController = inventorySlots[index].transform.GetComponent<InventorySlotUIController>();
        slotUIController.SetSlot(playerInventory.GetSlotDisplayInformation(index));
    }

    void DestroyAllSlots()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        inventorySlots = null;
        inventoryController.ClearAllRegistrations();
    }
}
