using UnityEngine;
using System;
using System.Collections.Generic;

public static class EventManager
{
    public static event Action<int[]> InventoryUpdateEvent;
    public static event System.Action<InventorySlotUIController> InventorySlotClickedEvent;
    public static event System.Action<InventorySlotUIController> InventorySlotClickedStackSelectionEvent;

    public static void TriggerInventoryUpdateEvent(int[] indicesToUpdate)
    {
        InventoryUpdateEvent?.Invoke(indicesToUpdate);
    }

    public static void TriggerInventorySlotClickedEvent(InventorySlotUIController slotUIController)
    {
        InventorySlotClickedEvent.Invoke(slotUIController);
    }

    public static void TriggerInventorySlotClickedStackSelectionEvent(InventorySlotUIController slotUIController)
    {
        InventorySlotClickedStackSelectionEvent.Invoke(slotUIController);
    }
}
