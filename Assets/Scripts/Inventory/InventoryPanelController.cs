using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class InventoryPanelController : MonoBehaviour
{
    [SerializeField] private Inventory playerInventory;
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
            playerInventory.RegisterUIInventorySlot(slotUIController, index);
        }

        // Now, put inventory items into their proper slots in the inventory panel, based on their index locations in the inventory itself
        for(int index = 0; index < playerInventory.InventorySize; ++index)
        {
            Inventory.InventoryEntry entry = playerInventory[index];
            if (entry == null)
            {
                continue;
            }
            InventorySlotUIController slotUIController = inventorySlots[index].transform.GetComponent<InventorySlotUIController>();
            slotUIController.SetSlot(entry);
        }
    }

    void UpdateDirtySlots(int[] indicesToUpdate)
    {
        foreach (int indexToUpdate in indicesToUpdate)
        {
            if (indexToUpdate >= 0)  // index -1 (Inventory.CursorSlotIndex) is used for the cursor inventory slot
            {
                UpdateSlot(indexToUpdate, playerInventory[indexToUpdate]);
            }
        }
    }

    void UpdateSlot(int index, Inventory.InventoryEntry newEntry)
    {
        InventorySlotUIController slotUIController = inventorySlots[index].transform.GetComponent<InventorySlotUIController>();
        if (newEntry == null)
        {
            slotUIController.EmptySlot();
        }
        else
        {
            slotUIController.SetSlot(newEntry);
        }
    }

    void DestroyAllSlots()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        inventorySlots = null;
    }
}
