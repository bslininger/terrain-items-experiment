using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int inventorySize = 30;

    private class InventoryEntry
    {
        public Item item;
        public int stackSize;
    }

    private List<InventoryEntry> slots = new List<InventoryEntry>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            PrintInventory();
        }
    }

    public void AddItem(Item item, int amountToAdd)
    {
        int amountLeftToAdd = amountToAdd;
        int maxStackSize = item.maxStack;

        if (!item.IsStackable)
        {
            while(amountLeftToAdd > 0)
            {
                slots.Add(new InventoryEntry { item = item, stackSize = 1 });
                amountLeftToAdd -= 1;
            }
            Debug.Log($"Added {amountToAdd} {item.itemName}(s) to inventory, kupo!");
            EventManager.TriggerInventoryUpdateEvent();
            return;
        }

        // Fill in any existing stacks
        foreach (InventoryEntry slot in slots)
        {
            if (slot.item == item && slot.stackSize < maxStackSize)
            {
                int remainingSpace = maxStackSize - slot.stackSize;
                int amountAddedToStack = Mathf.Min(amountLeftToAdd, remainingSpace);
                slot.stackSize += amountAddedToStack;
                amountLeftToAdd -= amountAddedToStack;
            }
            if (amountLeftToAdd < 1)
            {
                Debug.Log($"Added {amountToAdd} {item.itemName}(s) to inventory, kupo!");
                EventManager.TriggerInventoryUpdateEvent();
                return;
            }
        }

        // Add extra stacks if needed
        while (amountLeftToAdd > 0)
        {
            int thisStackSize = Mathf.Min(maxStackSize, amountLeftToAdd);
            slots.Add(new InventoryEntry { item = item, stackSize = thisStackSize });
            amountLeftToAdd -= thisStackSize;
        }

        EventManager.TriggerInventoryUpdateEvent();
        Debug.Log($"Added {amountToAdd} {item.itemName}(s) to inventory, kupo!");
    }

    public bool HasItem(Item item)
    {
        foreach (InventoryEntry slot in slots)
        {
            if (slot.item.Equals(item)) {
                return true;
            }
        }
        return false;
    }

    public void PrintInventory()
    {
        Debug.Log($"Inventory contents ({slots.Count} total item(s)):");
        foreach (InventoryEntry slot in slots)
        {
            Debug.Log($"- {slot.item.itemName} ({slot.stackSize})");
        }
    }

    public IEnumerable<(Item, int)> EnumerateInventory()
    {
        foreach (InventoryEntry slot in slots)
        {
            yield return (slot.item, slot.stackSize);
        }
    }

    public int InventorySize => inventorySize;
}
