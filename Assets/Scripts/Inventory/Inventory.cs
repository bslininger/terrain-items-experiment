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
        EventManager.InventorySlotClickedStackSelectionEvent += HandleUIInventorySlotClickedForStackSelection;
    }

    private void OnDisable()
    {
        EventManager.InventorySlotClickedEvent -= HandleUIInventorySlotClicked;
        EventManager.InventorySlotClickedStackSelectionEvent -= HandleUIInventorySlotClickedForStackSelection;
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

        InventoryEntry clickedEntry = inventoryEntries[index]; // same reference as inventoryEntries[index] (not a copy!) We move this reference around, or move items between it and the cursor inventory entry.
        itemCursorFollowerController.Activate();

        if (controller.SlotClickType == InventorySlotUIController.ClickType.Ctrl && !ItemInCursorSlot)
        {
            HandleUIInventorySlotClickedForSinglePull(clickedEntry, index);
            return;
        }

        if (!ItemInCursorSlot)
        {
            // Pull item from inventory onto cursor
            cursorInventoryEntry = clickedEntry;
            inventoryEntries[index] = null;
        }
        else // Item in cursor slot
        {
            if (clickedEntry == null) {
                // Empty inventory slot: move item from cursor to inventory slot
                inventoryEntries[index] = cursorInventoryEntry;
                cursorInventoryEntry = null;
            }
            else
            {
                if (cursorInventoryEntry.item != clickedEntry.item)
                {
                    // Different items: swap
                    inventoryEntries[index] = cursorInventoryEntry;
                    cursorInventoryEntry = clickedEntry;
                }
                else
                {
                    if (clickedEntry.stackSize < clickedEntry.item.maxStack)
                    {
                        // Same item, and there is room for more in its stack in the inventory slot: add to stack, and if stack fills, keep remainder on cursor
                        int stackOnCursor = cursorInventoryEntry.stackSize;
                        int remainingSpace = clickedEntry.item.maxStack - clickedEntry.stackSize;
                        int amountAddedToInventoryStack = Mathf.Min(stackOnCursor, remainingSpace);
                        inventoryEntries[index].AddToStack(amountAddedToInventoryStack);
                        if (stackOnCursor - amountAddedToInventoryStack > 0)
                        {
                            cursorInventoryEntry.RemoveFromStack(amountAddedToInventoryStack);
                        }
                        else
                        {
                            cursorInventoryEntry = null;
                        }
                    }
                    else
                    {
                        // Same item, but the inventory slot's stack is full: swap
                        inventoryEntries[index] = cursorInventoryEntry;
                        cursorInventoryEntry = clickedEntry;
                    }
                }
            }
        }
        RefreshSlots(CursorSlotIndex, index);
    }

    private void HandleUIInventorySlotClickedForSinglePull(InventoryEntry clickedEntry, int index)
    {
        // This function only runs if the clicked slot has a stackable item with at least 2 in it, so no need to check for stack size here.
        clickedEntry.RemoveFromStack(1);
        cursorInventoryEntry = new InventoryEntry(clickedEntry.item, 1);
        RefreshSlots(CursorSlotIndex, index);
    }

    private void HandleUIInventorySlotClickedForStackSelection(InventorySlotUIController controller)
    {
        if (!slotUIControllerToIndexMap.TryGetValue(controller, out int index))
        {
            Debug.LogWarning("Clicked slot not registered with Inventory!");
            return;
        }

        if (ItemInCursorSlot)
        {
            // Fall back to handling this as if it were a regular click.
            HandleUIInventorySlotClicked(controller);
            return;
        }

        InventoryEntry clickedEntry = inventoryEntries[index];
        Action<int> acceptButtonAction = (amountTaken) =>
        {
            itemCursorFollowerController.Activate();
            if (amountTaken == clickedEntry.stackSize)
            {
                cursorInventoryEntry = clickedEntry;
                inventoryEntries[index] = null;
            }
            else
            {
                clickedEntry.RemoveFromStack(amountTaken);
                cursorInventoryEntry = new InventoryEntry(clickedEntry.item, amountTaken);
            }
            RefreshSlots(CursorSlotIndex, index);
        };

        // Get location for stack size selector panel to open, then open it

        RectTransform inventorySlotRectTransform = controller.GetComponent<RectTransform>();
        Vector2 slotTransformSize = inventorySlotRectTransform.rect.size;  // (width, height) pair for the slot's size
        Vector2 slotTransformPivotOffset = inventorySlotRectTransform.pivot;  // Represents the pivot offset within the slot transform as a pair (x-offset, y-offset). The values range from 0 to 1, where 0 is the left (x) or bottom (y).
        Vector2 bottomRightOffset = new Vector2((1.0f - slotTransformPivotOffset.x) * slotTransformSize.x, (0.0f - slotTransformPivotOffset.y) * slotTransformSize.y); // Calculate the offset of the bottom-right corner from the pivot location
        RectTransform inventoryPanelRectTransform = inventorySlotRectTransform.GetComponentInParent<InventoryPanelController>().GetComponent<RectTransform>();
        Vector2 bottomRightCorner = inventoryPanelRectTransform.anchoredPosition + inventorySlotRectTransform.anchoredPosition + bottomRightOffset;
        Vector2 stackSizeSelectorPanelPosition = bottomRightCorner;

        UIManager.Instance.ShowStackSizeSelectorPanel(new InventoryEntry(clickedEntry), stackSizeSelectorPanelPosition, acceptButtonAction);
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
