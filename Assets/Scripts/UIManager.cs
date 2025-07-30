using System;
using UnityEngine;

[Flags]
public enum UIInputLock
{
    None = 0,
    InventoryInteraction = 1 << 0,
    All = InventoryInteraction
}
public class UIManager : MonoBehaviour, IInputLockProvider
{
    public static UIManager Instance { get; private set; }

    // Canvases
    [SerializeField] private Canvas inventoryUICanvas;

    // Prefabs
    [SerializeField] private GameObject stackSizeSelectorPrefab;

    // Controllers
    private StackSizeSelectorPanelController activeStackSizeSelectorPanelController;

    // Accessors
    public Canvas InventoryUICanvas => inventoryUICanvas;

    private UIInputLock activeLocks = UIInputLock.None;

    private void Awake()
    {
        // Keeps this UIManager as a singleton
        if (Instance != null)
        {
            Debug.LogWarning("Another UIManager tried to spawn and was destroyed!");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }



    public void ShowStackSizeSelectorPanel(Inventory.InventoryEntry inventoryEntry, Vector2 location, Action<int> acceptButtonAction)
    {
        if (StackSizeSelectorPanelOpen)
        {
            Debug.Log("A stack size selector panel is already open somewhere. Not opening another one.");
            return;
        }

        if (InputLocked(UIInputLock.InventoryInteraction))
        {
            Debug.Log("Can't open stack size selector panel because the InventoryInteraction UIInputLock is already locked.");
            return;
        }

        GameObject panelInstance = Instantiate(stackSizeSelectorPrefab, inventoryUICanvas.transform);
        StackSizeSelectorPanelController stackSizeSelectorPanelController = panelInstance.GetComponent<StackSizeSelectorPanelController>();
        RectTransform stackSizeSelectorPanelRectTransform = panelInstance.GetComponent<RectTransform>();
        stackSizeSelectorPanelRectTransform.anchoredPosition = location + new Vector2(stackSizeSelectorPanelRectTransform.rect.width / 2 + 10.0f, 0.0f);

        stackSizeSelectorPanelController.SetInventoryEntry(inventoryEntry);

        stackSizeSelectorPanelController.SetAcceptAction((int amount) =>
        {
            acceptButtonAction?.Invoke(amount);
            CloseStackSizeSelectorPanel();
        });
        stackSizeSelectorPanelController.SetCancelAction(() =>
        {
            CloseStackSizeSelectorPanel();
        });

        AddInputLock(UIInputLock.InventoryInteraction);
        activeStackSizeSelectorPanelController = stackSizeSelectorPanelController;
    }

    public void CloseStackSizeSelectorPanel()
    {
        if (activeStackSizeSelectorPanelController != null)
        {
            Destroy(activeStackSizeSelectorPanelController.gameObject);
            activeStackSizeSelectorPanelController = null;
            RemoveInputLock(UIInputLock.InventoryInteraction);
        }
    }

    public bool StackSizeSelectorPanelOpen => activeStackSizeSelectorPanelController != null;

    #region Locks
    public void AddInputLock(UIInputLock lockType)
    {
        if (InputLocked(lockType))
        {
            Debug.LogError($"Attempted to lock {lockType}, which is already locked."); // This lock system allows only one entity to lock a specific lock at a time.
            return;
        }
        activeLocks |= lockType;
    }

    public void RemoveInputLock(UIInputLock lockType)
    {
        if (!InputLocked(lockType))
        {
            Debug.LogWarning($"Attempted to unlock {lockType}, but it wasn't locked. Was this intentional?");
            return;
        }
        activeLocks &= ~lockType;
    }

    #region IInputLockProvider
    public bool InputLocked(UIInputLock lockType)
    {
        return (activeLocks & lockType) != 0;
    }
    #endregion

    #endregion
}
