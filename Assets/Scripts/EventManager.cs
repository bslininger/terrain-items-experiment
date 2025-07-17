using UnityEngine;
using System;
using System.Collections.Generic;

public class EventManager : MonoBehaviour
{
    public static event Action<int[]> InventoryUpdateEvent;
    public static event System.Action<InventorySlotUIController> InventorySlotClickedEvent;

    public static void TriggerInventoryUpdateEvent(int[] indicesToUpdate)
    {
        InventoryUpdateEvent?.Invoke(indicesToUpdate);
    }

    public static void TriggerInventorySlotClickedEvent(InventorySlotUIController slotUIController)
    {
        InventorySlotClickedEvent.Invoke(slotUIController);
    }
}
