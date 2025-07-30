using UnityEngine;

public interface IItemDisplayInformation
{
    string ItemName { get; }
    int StackSize { get; }
    int MaxStackSize { get; }
    UnityEngine.Sprite Icon { get; }
}

public interface IInputLockProvider
{
    bool InputLocked(UIInputLock lockType);
}
