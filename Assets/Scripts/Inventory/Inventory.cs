using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int inventorySize = 30;
    [SerializeField] private InventoryItemCursorFollower itemCursorFollowerController;

    private Dictionary<InventorySlotUIController, int> slotUIControllerToIndexMap;


    public class InventoryEntry : IItemDisplayInformation
    {
        public Item item { get; }
        public int stackSize { get; private set; }

        public string ItemName => item.itemName;
        public int StackSize => stackSize;
        public int MaxStackSize => item.maxStack;
        public Sprite Icon => item.icon;

        public InventoryEntry(Item item, int stackSize)
        {
            if (item == null)
                throw new System.ArgumentNullException(nameof(item), "Item can not be null");
            if (stackSize <= 0)
                throw new System.ArgumentException("Stack size must be at least 1");
            if (stackSize > item.maxStack)
                throw new System.ArgumentException($"Given stack size {stackSize} exceeds item's maximum allowable stack size of {item.maxStack}");

            this.item = item;
            this.stackSize = stackSize;
        }

        public InventoryEntry(InventoryEntry other)
        {
            if (other == null)
                throw new System.ArgumentNullException("Tried to copy frum a null InventoryEntry");
            item = other.item;
            stackSize = other.stackSize;
        }

        public void AddToStack(int amountToAdd)
        {
            if (amountToAdd < 0)
                throw new System.ArgumentException("Can only add nonnegative amounts to the stack size");

            int newSize = stackSize + amountToAdd;
            if (newSize > item.maxStack)
                throw new System.InvalidOperationException($"Adding {amountToAdd} to the item's stack results in a stack size of {newSize}, larger than the maximum allowable stack size of {item.maxStack}");

            stackSize = newSize;
        }

        public void RemoveFromStack(int amount)
        {
            if (amount < 0)
                throw new System.ArgumentException("Can only add nonnegative amounts from the stack size");

            int newSize = stackSize - amount;
            if (newSize <= 0)
                throw new System.InvalidOperationException($"Stack size can not be reduced below 1; attempted to set the stack size to {newSize}");

            stackSize = newSize;
        }

        public void SetStackSize(int newStackSize)
        {
            if (newStackSize < 1 || newStackSize > item.maxStack)
                throw new System.ArgumentException($"Tried to set stack size to {newStackSize} which is out of bounds (1 to {item.maxStack}");
            stackSize = newStackSize;
        }
    }

    private InventoryEntry[] inventoryEntries;
    private InventoryEntry cursorInventoryEntry;

    private void Awake()
    {
        inventoryEntries = new InventoryEntry[inventorySize];
        slotUIControllerToIndexMap = new Dictionary<InventorySlotUIController, int>();
    }

    private void Update()
    {
    }

    private void OnEnable()
    {
        EventManager.InventorySlotClickedEvent += HandleUIInventorySlotClicked;
    }

    private void OnDisable()
    {
        EventManager.InventorySlotClickedEvent -= HandleUIInventorySlotClicked;
    }

    private void RefreshSlots(params int[] indices)
    {
        EventManager.TriggerInventoryUpdateEvent(indices);
    }
    private void HandleUIInventorySlotClicked(InventorySlotUIController controller)
    {
        if (!slotUIControllerToIndexMap.TryGetValue(controller, out int index))
        {
            Debug.LogWarning("Clicked slot not registered with Inventory!");
            return;
        }

        itemCursorFollowerController.Activate();

        // Special keypresses: ctrl-click to pick 1 item from the stack, shift-click to pick up a specified number from the stack
        if (controller.SlotClickType == InventorySlotUIController.ClickType.Ctrl && !ItemInCursorSlot)
        {
            HandleUIInventorySlotClickedForSinglePull(index);
            return;
        }
        if (controller.SlotClickType == InventorySlotUIController.ClickType.Shift && !ItemInCursorSlot)
        {
            HandleUIInventorySlotClickedForStackSelection(controller, index);
            return;
        }

        InteractWithInventorySlot(index);
    }

    private void HandleUIInventorySlotClickedForSinglePull(int index)
    {
        TakeFromSlotIntoCursor(index, 1);
    }

    private void HandleUIInventorySlotClickedForStackSelection(InventorySlotUIController controller, int index)
    {
        Action<int> acceptButtonAction = (amountTaken) =>
        {
            itemCursorFollowerController.Activate();
            TakeFromSlotIntoCursor(index, amountTaken);
        };

        // Get location for stack size selector panel to open, then open it

        RectTransform inventorySlotRectTransform = controller.GetComponent<RectTransform>();
        Vector2 slotTransformSize = inventorySlotRectTransform.rect.size;  // (width, height) pair for the slot's size
        Vector2 slotTransformPivotOffset = inventorySlotRectTransform.pivot;  // Represents the pivot offset within the slot transform as a pair (x-offset, y-offset). The values range from 0 to 1, where 0 is the left (x) or bottom (y).
        Vector2 bottomRightOffset = new Vector2((1.0f - slotTransformPivotOffset.x) * slotTransformSize.x, (0.0f - slotTransformPivotOffset.y) * slotTransformSize.y); // Calculate the offset of the bottom-right corner from the pivot location
        RectTransform inventoryPanelRectTransform = inventorySlotRectTransform.GetComponentInParent<InventoryPanelController>().GetComponent<RectTransform>();
        Vector2 bottomRightCorner = inventoryPanelRectTransform.anchoredPosition + inventorySlotRectTransform.anchoredPosition + bottomRightOffset;
        Vector2 stackSizeSelectorPanelPosition = bottomRightCorner;

        UIManager.Instance.ShowStackSizeSelectorPanel(new InventoryEntry(inventoryEntries[index]), stackSizeSelectorPanelPosition, acceptButtonAction);
    }

    private void InteractWithInventorySlot(int index)
    {
        InventoryEntry clickedEntry = inventoryEntries[index]; // same reference as inventoryEntries[index] (not a copy!) We move this reference around, or move items between it and the cursor inventory entry.

        if (!ItemInCursorSlot)
        {
            if (clickedEntry != null)
            {
                // Pull item from inventory onto cursor
                TakeFromSlotIntoCursor(index, clickedEntry.stackSize);
            }
            // Otherwise, do nothing.
            return;
        }
        else // Item in cursor slot
        {
            if (clickedEntry == null)
            {
                // Empty inventory slot: move item from cursor to inventory slot
                PlaceFromCursorIntoSlot(index);
                return;
            }
            else
            {
                if (cursorInventoryEntry.item != clickedEntry.item)
                {
                    // Different items: swap
                    SwapCursorWithSlot(index);
                    return;
                }
                else
                {
                    if (clickedEntry.stackSize < clickedEntry.item.maxStack)
                    {
                        // Same item, and there is room for more in its stack in the inventory slot: add to stack, and if stack fills, keep remainder on cursor
                        MergeFromCursorIntoSlot(index);
                        return;
                    }
                    else
                    {
                        // Same item, but the inventory slot's stack is full: swap
                        SwapCursorWithSlot(index);
                        return;
                    }
                }
            }
        }
    }

    private void TakeFromSlotIntoCursor(int index, int amount)
    {
        if (index < 0 || index >= inventoryEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(index), $"Inventory index must be between 0 and {inventoryEntries.Length - 1}; index given was {index}");
        if (ItemInCursorSlot)
            throw new InvalidOperationException("Tried to add an item to the cursor slot when it was already occupied.");

        InventoryEntry sourceEntry = inventoryEntries[index]; // same reference as inventoryEntries[index] (not a copy!) We move this reference around, or move items between it and the cursor inventory entry.

        if (sourceEntry == null)
            throw new InvalidOperationException($"Tried to pull from inventory index {index}, but it was empty.");
        if (amount <= 0 || amount > sourceEntry.stackSize)
            throw new ArgumentOutOfRangeException(nameof(amount), $"Amount to pull must be positive and less than or equal to the amount currently in the inventory slot ({sourceEntry.stackSize}); was given {amount}");

        if (amount < sourceEntry.stackSize)
        {
            cursorInventoryEntry = new InventoryEntry(sourceEntry.item, amount);
            sourceEntry.RemoveFromStack(amount);
        }
        else
        {
            cursorInventoryEntry = sourceEntry;
            inventoryEntries[index] = null;
        }
        RefreshSlots(CursorSlotIndex, index); // for now
    }

    private void PlaceFromCursorIntoSlot(int index)
    {
        if (index < 0 || index >= inventoryEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(index), $"Inventory index must be between 0 and {inventoryEntries.Length - 1}; index given was {index}");
        if (!ItemInCursorSlot)
            throw new InvalidOperationException("Tried to place an item from the cursor slot when it was empty.");

        InventoryEntry destinationEntry = inventoryEntries[index]; // same reference as inventoryEntries[index] (not a copy!) We move this reference around, or move items between it and the cursor inventory entry.

        if (destinationEntry != null)
            throw new InvalidOperationException($"Tried to place an item into inventory index {index}, but it was already occupied.");

        inventoryEntries[index] = cursorInventoryEntry;
        cursorInventoryEntry = null;

        RefreshSlots(CursorSlotIndex, index); // for now
    }

    private void MergeFromCursorIntoSlot(int index)
    {
        if (index < 0 || index >= inventoryEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(index), $"Inventory index must be between 0 and {inventoryEntries.Length - 1}; index given was {index}");
        if (!ItemInCursorSlot)
            throw new InvalidOperationException("Tried to place an item from the cursor slot when it was empty.");

        InventoryEntry destinationEntry = inventoryEntries[index]; // same reference as inventoryEntries[index] (not a copy!) We move this reference around, or move items between it and the cursor inventory entry.

        if (destinationEntry == null)
            throw new InvalidOperationException($"Tried to merge the item in the cursor slot into inventory index {index}, but the inventory slot was empty.");
        if (destinationEntry.item != cursorInventoryEntry.item)
            throw new InvalidOperationException($"Tried to merge the item in the cursor slot into inventory index {index}, but the cursor and inventory slots did not contain the same item.");
        if (destinationEntry.stackSize >= destinationEntry.item.maxStack)
            throw new InvalidOperationException($"Tried to merge the item in the cursor slot into inventory index {index}, but the inventory slot's stack was already full.");

        int stackOnCursor = cursorInventoryEntry.stackSize;
        int remainingSpace = destinationEntry.item.maxStack - destinationEntry.stackSize;
        int amountAddedToInventoryStack = Mathf.Min(stackOnCursor, remainingSpace);
        destinationEntry.AddToStack(amountAddedToInventoryStack);
        if (stackOnCursor - amountAddedToInventoryStack > 0)
        {
            cursorInventoryEntry.RemoveFromStack(amountAddedToInventoryStack);
        }
        else
        {
            cursorInventoryEntry = null;
        }

        RefreshSlots(CursorSlotIndex, index); // for now
    }

    private void SwapCursorWithSlot(int index)
    {
        if (index < 0 || index >= inventoryEntries.Length)
            throw new ArgumentOutOfRangeException(nameof(index), $"Inventory index must be between 0 and {inventoryEntries.Length - 1}; index given was {index}");
        if (!ItemInCursorSlot)
            throw new InvalidOperationException("Tried to place an item from the cursor slot when it was empty.");

        InventoryEntry clickedInventoryEntry = inventoryEntries[index]; // same reference as inventoryEntries[index] (not a copy!) We move this reference around, or move items between it and the cursor inventory entry.

        if (clickedInventoryEntry == null)
            throw new InvalidOperationException($"Tried to swap the cursor slot with inventory index {index}, but the inventory slot was empty.");

        inventoryEntries[index] = cursorInventoryEntry;
        cursorInventoryEntry = clickedInventoryEntry;

        RefreshSlots(CursorSlotIndex, index); // for now
    }

    public void AddItem(InventoryEntry entryToAdd, int? inventoryEntryIndexChoice = null)
    {
        Item item = entryToAdd.item;
        int amountToAdd = entryToAdd.stackSize;
        if (amountToAdd < 1)
        {
            return;
        }
        int amountLeftToAdd = amountToAdd;
        int maxStackSize = item.IsStackable ? item.maxStack : 1;
        List<int> indicesToUpdate = new List<int>();

        // First: try to put all items in the entry at the chosen index. If that is successful, then return early.
        if (inventoryEntryIndexChoice.HasValue && inventoryEntryIndexChoice.Value < inventorySize)
        {
            int inventoryEntryIndex = inventoryEntryIndexChoice.Value;
            if (inventoryEntries[inventoryEntryIndex] == null)
            {
                // Option 1: The chosen inventory slot is empty
                int amountAddedToStack = Mathf.Min(amountLeftToAdd, maxStackSize);
                inventoryEntries[inventoryEntryIndex] = new InventoryEntry(item, amountAddedToStack);
                indicesToUpdate.Add(inventoryEntryIndex);
                amountLeftToAdd -= amountAddedToStack;
            }
            else if (inventoryEntries[inventoryEntryIndex].item == item && amountLeftToAdd + inventoryEntries[inventoryEntryIndex].stackSize <= item.maxStack)
            {
                // Option 2: The chosen inventory slot has the same item as the one being added, and the stack at that location isn't full.
                int remainingSpace = maxStackSize - inventoryEntries[inventoryEntryIndex].stackSize;
                int amountAddedToStack = Mathf.Min(amountLeftToAdd, remainingSpace);
                inventoryEntries[inventoryEntryIndex].AddToStack(amountAddedToStack);
                indicesToUpdate.Add(inventoryEntryIndex);
                amountLeftToAdd -= amountAddedToStack;
            }
            if (amountLeftToAdd < 1)
            {
                RefreshSlots(indicesToUpdate.ToArray());
                return;
            }
        }

        // Now, fill in any existing stacks
        for (int index = 0; index < inventorySize; ++index)
        {
            InventoryEntry entry = inventoryEntries[index];
            if (entry != null && entry.item == item && entry.stackSize < maxStackSize)
            {
                int remainingSpace = maxStackSize - entry.stackSize;
                int amountAddedToStack = Mathf.Min(amountLeftToAdd, remainingSpace);
                entry.AddToStack(amountAddedToStack);
                indicesToUpdate.Add(index);
                amountLeftToAdd -= amountAddedToStack;
            }
            if (amountLeftToAdd < 1)
            {
                RefreshSlots(indicesToUpdate.ToArray());
                return;
            }
        }

        // If there are still items to add, add them in empty entries in order from beginning to end.
        Queue<int> emptyEntryIndices = new Queue<int>();
        for (int index = 0; index < inventorySize; ++index)
        {
            if (inventoryEntries[index] == null)
            {
                emptyEntryIndices.Enqueue(index);
            }
        }

        while (amountLeftToAdd > 0 && emptyEntryIndices.Count > 0)
        {
            int index = emptyEntryIndices.Dequeue();
            int thisStackSize = Mathf.Min(maxStackSize, amountLeftToAdd);
            inventoryEntries[index] = new InventoryEntry(item, thisStackSize);
            indicesToUpdate.Add(index);
            amountLeftToAdd -= thisStackSize;
        }

        if (amountLeftToAdd > 0)
        {
            Debug.LogWarning($"Tried to add more of the item \"{item.itemName}\" but there was no space left, so the remaining {amountLeftToAdd} item(s) were lost!");
        }

        RefreshSlots(indicesToUpdate.ToArray());
        return;
    }

    public bool HasItem(InventoryEntry entryToCheck)
    {
        Item item = entryToCheck.item;
        // The entry's stackSize is currently not checked here.
        // If we want to check if we have at least a certain amount of an item, a new function will be written which has the amount to look for that as a parameter.
        foreach (InventoryEntry entry in inventoryEntries)
        {
            if (entry != null && entry.item == item)
            {
                return true;
            }
        }
        return false;
    }

    public void RegisterUIInventorySlot(InventorySlotUIController slotUIController, int index)
    {
        slotUIControllerToIndexMap[slotUIController] = index;
    }

    public void PutInventoryEntryInCursorSlot(InventoryEntry inventoryEntry)
    {
        if (cursorInventoryEntry != null)
        {
            Debug.LogWarning("Tried to put something in the cursor inventory slot while it already had something in it.");
        }
        itemCursorFollowerController.Activate();
        cursorInventoryEntry = inventoryEntry;
        RefreshSlots(CursorSlotIndex);
    }

    public int InventorySize => inventorySize;
    public InventoryEntry this[int index]
    {
        get
        {
            if (index < CursorSlotIndex || index >= inventoryEntries.Length)
            {
                throw new System.IndexOutOfRangeException($"Tried to access inventory index {index}; out of range");
            }
            if (index == CursorSlotIndex)
            {
                return cursorInventoryEntry;
            }

            return inventoryEntries[index];
        }
    }
    public const int CursorSlotIndex = -1;
    public bool ItemInCursorSlot => cursorInventoryEntry != null;
}
