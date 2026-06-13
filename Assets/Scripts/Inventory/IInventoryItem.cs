using System;
using UnityEngine;

public interface IInventoryItem
{
    string DisplayName { get; }
    int MaxStackSize { get; }
    Sprite Icon { get; }
}
