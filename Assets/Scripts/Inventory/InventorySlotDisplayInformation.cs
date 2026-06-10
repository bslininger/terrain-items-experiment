using System;
using UnityEngine;

public readonly struct InventorySlotDisplayInformation
{
    public int SlotIndex { get; }
    public bool HasItem { get; }
    public string ItemName { get; }
    public Sprite Icon { get; }
    public int StackSize { get; }
    public int MaxStackSize { get; }

    private InventorySlotDisplayInformation(int slotIndex, bool hasItem, string itemName, Sprite icon, int stackSize, int maxStackSize)
    {
        if (slotIndex < 0 && slotIndex != Inventory.CursorSlotIndex)
            throw new ArgumentOutOfRangeException(nameof(slotIndex), $"Slot index must be non-negative or the special value of Inventory.CursorSlotIndex ({Inventory.CursorSlotIndex}).");
        if (!hasItem && !(itemName == null && icon == null && stackSize == 0 && maxStackSize == 0))
            throw new ArgumentException("An empty slot should have null item name and sprite fields along with stack size fields of 0.");

        SlotIndex = slotIndex;
        HasItem = hasItem;
        ItemName = itemName;
        Icon = icon;
        StackSize = stackSize;
        MaxStackSize = maxStackSize;
    }

    // Factory methods

    public static InventorySlotDisplayInformation Empty(int slotIndex)
    {
        return new InventorySlotDisplayInformation(slotIndex, false, null, null, 0, 0);
    }

    public static InventorySlotDisplayInformation Occupied(int slotIndex, string itemName, Sprite icon, int stackSize, int maxStackSize)
    {
        if (itemName == null)
            throw new ArgumentNullException(nameof(itemName), "Item name must be non-null for an occupied slot.");
        if (icon == null)
            throw new ArgumentNullException(nameof(icon), "Item icon sprite must be non-null for an occupied slot.");
        if (stackSize <= 0)
            throw new ArgumentException("Item stack size must be positive for an occupied slot.");
        if (maxStackSize <= 0)
            throw new ArgumentException("Item maximum stack size must be positive for an occupied slot.");
        if (stackSize > maxStackSize)
            throw new ArgumentException("The item's stack size is higher than the allowed maximum stack size.");

        return new InventorySlotDisplayInformation(slotIndex, true, itemName, icon, stackSize, maxStackSize);
    }
}
