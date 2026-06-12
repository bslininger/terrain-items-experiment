using System;
using System.Collections.Generic;
using System.Linq;

public readonly struct InventoryOperationResult
{
    public enum ResultType
    {
        NoOperation,
        ItemFullyAdded,
        ItemPartiallyAdded,
        NoSpace,
        PickupToCursor,
        PlaceFromCursor,
        SwapWithCursor,
        MergeFromCursor,
    }

    public ResultType OperationResultType { get; }

    public bool CursorSlotChanged { get; }
    public int LeftoverItemCount { get; }  // Count of items that couldn't fit in an inventory because it ran out of room; the "overflow" item count.
    public IReadOnlyList<int> ChangedSlotIndices { get; }

    private InventoryOperationResult(ResultType operationResultType, bool cursorSlotChanged, int leftoverItemCount, params int[] changedSlotIndices)
    {
        if (changedSlotIndices == null)
            throw new ArgumentNullException(nameof(changedSlotIndices));
        if (leftoverItemCount < 0)
            throw new ArgumentException("Leftover item count must be non-negative.");

        OperationResultType = operationResultType;
        CursorSlotChanged = cursorSlotChanged;
        LeftoverItemCount = leftoverItemCount;
        ChangedSlotIndices = Array.AsReadOnly(changedSlotIndices.ToArray());
    }

    // Factory methods
    public static InventoryOperationResult NoOperation()
    {
        return new InventoryOperationResult(ResultType.NoOperation, false, 0);
    }

    public static InventoryOperationResult ItemFullyAdded(params int[] changedSlotIndices)
    {
        return new InventoryOperationResult(ResultType.ItemFullyAdded, false, 0, changedSlotIndices);
    }

    public static InventoryOperationResult ItemPartiallyAdded(int leftoverItemCount, params int[] changedSlotIndices)
    {
        return new InventoryOperationResult(ResultType.ItemPartiallyAdded, false, leftoverItemCount, changedSlotIndices);
    }

    public static InventoryOperationResult NoSpace(int leftoverItemCount)
    {
        return new InventoryOperationResult(ResultType.NoSpace, false, leftoverItemCount);
    }

    public static InventoryOperationResult PickupToCursor(int? changedSlotIndex)
    {
        // A null changedSlotIndex represents no inventory slots changing, just the cursor slot (for example, when receiving an item to the cursor, or picking one up off the ground)
        if (changedSlotIndex.HasValue)
            return new InventoryOperationResult(ResultType.PickupToCursor, true, 0, changedSlotIndex.Value);
        return new InventoryOperationResult(ResultType.PickupToCursor, true, 0);
    }

    public static InventoryOperationResult PlaceFromCursor(int changedSlotIndex)
    {
        return new InventoryOperationResult(ResultType.PlaceFromCursor, true, 0, changedSlotIndex);
    }

    public static InventoryOperationResult SwapWithCursor(int changedSlotIndex)
    {
        return new InventoryOperationResult(ResultType.SwapWithCursor, true, 0, changedSlotIndex);
    }

    public static InventoryOperationResult MergeFromCursor(int changedSlotIndex)
    {
        return new InventoryOperationResult(ResultType.MergeFromCursor, true, 0, changedSlotIndex);
    }
}
