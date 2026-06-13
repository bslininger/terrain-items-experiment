using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private Inventory playerInventory;
    private Dictionary<InventorySlotUIController, int> slotUIControllerToIndexMap;

    private void Awake()
    {
        slotUIControllerToIndexMap = new Dictionary<InventorySlotUIController, int>();
    }

    private void OnEnable()
    {
        EventManager.InventorySlotClickedEvent += HandleUIInventorySlotClicked;
    }

    private void OnDisable()
    {
        EventManager.InventorySlotClickedEvent -= HandleUIInventorySlotClicked;
    }

    public void RegisterUIInventorySlot(InventorySlotUIController slotUIController, int index)
    {
        slotUIControllerToIndexMap[slotUIController] = index;
    }

    public void UnregisterUIInventorySlot(InventorySlotUIController slotUIController)
    {
        slotUIControllerToIndexMap.Remove(slotUIController);
    }

    public void ClearAllRegistrations()
    {
        slotUIControllerToIndexMap.Clear();
    }

    private void PublishInventoryUpdate(InventoryOperationResult inventoryOperationResult)
    {
        if (inventoryOperationResult.OperationResultType == InventoryOperationResult.ResultType.NoOperation || (!inventoryOperationResult.CursorSlotChanged && inventoryOperationResult.ChangedSlotIndices.Count == 0))
            return;

        if (!inventoryOperationResult.CursorSlotChanged)
            PublishInventoryUpdate(inventoryOperationResult.ChangedSlotIndices.ToArray());
        else
            PublishInventoryUpdate(inventoryOperationResult.ChangedSlotIndices.Prepend(Inventory.CursorSlotIndex).ToArray());
    }

    private void PublishInventoryUpdate(params int[] indices)
    {
        EventManager.TriggerInventoryUpdateEvent(indices);
    }

    public InventoryOperationResult HandleAddItemToInventory(Item item, int amountToAdd, int? slotIndexChoice = null, bool allowOverflowOutsideChosenSlot = true)
    {
        if (item == null)
        {
            Debug.LogWarning("Tried to add a null item to the inventory.");
            return InventoryOperationResult.NoOperation();
        }
        InventoryOperationResult inventoryOperationResult = playerInventory.AddItem(item, amountToAdd, slotIndexChoice, allowOverflowOutsideChosenSlot);
        if (inventoryOperationResult.OperationResultType == InventoryOperationResult.ResultType.ItemPartiallyAdded || inventoryOperationResult.OperationResultType == InventoryOperationResult.ResultType.NoSpace)
            Debug.LogWarning($"Out of {amountToAdd} items to add, {inventoryOperationResult.LeftoverItemCount} were lost due to not having space in the inventory. (Overflow to other slots was {(allowOverflowOutsideChosenSlot ? "" : "not ")}enabled.)");
        PublishInventoryUpdate(inventoryOperationResult);
        return inventoryOperationResult;
    }

    public InventoryOperationResult HandlePutItemInCursorSlot(Item item, int amount)
    {
        if (item == null)
        {
            Debug.LogWarning("Tried to add a null item to the cursor.");
            return InventoryOperationResult.NoOperation();
        }
        InventoryOperationResult inventoryOperationResult = playerInventory.PutItemInCursorSlot(item, amount);
        if (inventoryOperationResult.OperationResultType == InventoryOperationResult.ResultType.NoSpace)
            Debug.LogWarning("Tried to put something in the cursor inventory slot while it already had something in it.");
        else if (inventoryOperationResult.OperationResultType == InventoryOperationResult.ResultType.NoOperation || !inventoryOperationResult.CursorSlotChanged)
            Debug.LogWarning("No change when attempting to add an item to the cursor slot.");

        PublishInventoryUpdate(inventoryOperationResult);
        return inventoryOperationResult;
    }

    private void HandleUIInventorySlotClicked(InventorySlotUIController controller)
    {
        if (!slotUIControllerToIndexMap.TryGetValue(controller, out int index))
        {
            Debug.LogWarning("Clicked slot not registered with Inventory!");
            return;
        }

        InventoryOperationResult inventoryOperationResult;

        // Special keypresses: ctrl-click to pick 1 item from the stack, shift-click to pick up a specified number from the stack
        if (controller.SlotClickType == InventorySlotUIController.ClickType.Ctrl && !playerInventory.ItemInCursorSlot)
            inventoryOperationResult = HandleUIInventorySlotClickedForSinglePull(index);
        else if (controller.SlotClickType == InventorySlotUIController.ClickType.Shift && !playerInventory.ItemInCursorSlot)
        {
            HandleUIInventorySlotClickedForStackSelection(controller, index);
            return;
        }
        else
            inventoryOperationResult = playerInventory.InteractWithSlot(index);

        PublishInventoryUpdate(inventoryOperationResult);
    }

    private InventoryOperationResult HandleUIInventorySlotClickedForSinglePull(int index)
    {
        return playerInventory.TakeFromSlotIntoCursor(index, 1);
    }

    private void HandleUIInventorySlotClickedForStackSelection(InventorySlotUIController controller, int index)
    {
        Action<int> acceptButtonAction = (amountTaken) => OnStackSizeSelectorAccepted(index, amountTaken);

        // Get location for stack size selector panel to open, then open it
        Vector2 stackSizeSelectorPanelPosition = CalculateStackSelectionPanelPosition(controller);
        UIManager.Instance.ShowStackSizeSelectorPanel(playerInventory.GetSlotDisplayInformation(index), stackSizeSelectorPanelPosition, acceptButtonAction);
    }

    private Vector2 CalculateStackSelectionPanelPosition(InventorySlotUIController controller)
    {
        RectTransform inventorySlotRectTransform = controller.GetComponent<RectTransform>();
        Vector2 slotTransformSize = inventorySlotRectTransform.rect.size;  // (width, height) pair for the slot's size
        Vector2 slotTransformPivotOffset = inventorySlotRectTransform.pivot;  // Represents the pivot offset within the slot transform as a pair (x-offset, y-offset). The values range from 0 to 1, where 0 is the left (x) or bottom (y).
        Vector2 bottomRightOffset = new Vector2((1.0f - slotTransformPivotOffset.x) * slotTransformSize.x, (0.0f - slotTransformPivotOffset.y) * slotTransformSize.y); // Calculate the offset of the bottom-right corner from the pivot location
        RectTransform inventoryPanelRectTransform = inventorySlotRectTransform.GetComponentInParent<InventoryPanelController>().GetComponent<RectTransform>();
        Vector2 bottomRightCorner = inventoryPanelRectTransform.anchoredPosition + inventorySlotRectTransform.anchoredPosition + bottomRightOffset;
        return bottomRightCorner;
    }

    private void OnStackSizeSelectorAccepted(int index, int amountTaken)
    {
        InventoryOperationResult inventoryOperationResult = playerInventory.TakeFromSlotIntoCursor(index, amountTaken);
        PublishInventoryUpdate(inventoryOperationResult);
    }
}
