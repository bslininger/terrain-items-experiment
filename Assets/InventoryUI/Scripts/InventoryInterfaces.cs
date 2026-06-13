using UnityEngine;

public interface IInputLockProvider
{
    bool InputLocked(UIInputLock lockType);
}
