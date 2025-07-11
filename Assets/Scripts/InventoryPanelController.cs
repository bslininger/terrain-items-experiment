using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryPanelController : MonoBehaviour
{
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private GameObject slotPrefab;

    private void OnEnable()
    {
        EventManager.InventoryUpdateEvent += PopulatePanelFromInventory;
    }

    private void OnDisable()
    {
        EventManager.InventoryUpdateEvent -= PopulatePanelFromInventory;
    }

    void PopulatePanelFromInventory()
    {
        int filledSlots = 0;
        ClearAllSlots();
        foreach ((Item, int) slot in playerInventory.EnumerateInventory())
        {
            GameObject newInventorySlot = Instantiate(slotPrefab, transform);
            Transform itemImageTransform = newInventorySlot.transform.GetChild(0);
            Transform stackTextTransform = newInventorySlot.transform.GetChild(1);
            itemImageTransform.GetComponent<Image>().sprite = slot.Item1.icon;
            itemImageTransform.gameObject.SetActive(true);
            if (slot.Item2 > 1)
            {
                stackTextTransform.GetComponent<TextMeshProUGUI>().text = slot.Item2.ToString();
                stackTextTransform.gameObject.SetActive(true);
            }
            filledSlots += 1;
        }

        Debug.Log($"Filled slots: {filledSlots}");

        for (int i = filledSlots + 1; i <= playerInventory.InventorySize; ++i)
            Instantiate(slotPrefab, transform);
    }

    void ClearAllSlots()
    {
        for (int i = transform.childCount - 1; i >= 0; --i)
        {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }
}
