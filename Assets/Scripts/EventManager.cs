using UnityEngine;
using System;

public class EventManager : MonoBehaviour
{
    public static event Action InventoryUpdateEvent;

    public static void TriggerInventoryUpdateEvent()
    {
        InventoryUpdateEvent?.Invoke();
    }
}
