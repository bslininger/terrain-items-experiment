using UnityEngine;

public class UIManager : MonoBehaviour
{
    // Canvases
    [SerializeField] private Canvas inventoryUICanvas;

    // Prefabs
    [SerializeField] private GameObject stackSizeSelectorPrefab;

    public void ShowStackSizeSelectorPanel(Inventory.InventoryEntry inventoryEntry)
    {
        GameObject panelInstance = Instantiate(stackSizeSelectorPrefab, inventoryUICanvas.transform);
        StackSizeSelectorPanelController stackSizeSelectorPanelController = panelInstance.GetComponent<StackSizeSelectorPanelController>();
        stackSizeSelectorPanelController.SetInventoryEntry(inventoryEntry);
    }

}
