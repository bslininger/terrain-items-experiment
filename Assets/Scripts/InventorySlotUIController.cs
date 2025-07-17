using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUIController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemSpriteImage;
    [SerializeField] private TextMeshProUGUI stackText;

    private Inventory.InventoryEntry inventoryEntry;

    public void SetSlot(Inventory.InventoryEntry entry)
    {
        inventoryEntry = entry;
        itemSpriteImage.sprite = inventoryEntry.item.icon;
        stackText.text = inventoryEntry.stackSize.ToString();
        itemSpriteImage.gameObject.SetActive(true);
        stackText.gameObject.SetActive(inventoryEntry.stackSize > 1);  // Only show the stack value on the corner of the icon if the stack has more than 1 in it.
    }

    public void EmptySlot()
    {
        inventoryEntry = null;
        itemSpriteImage.sprite = null;
        stackText.text = "0";
        itemSpriteImage.gameObject.SetActive(false);
        stackText.gameObject.SetActive(false);
    }

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    if (inventoryEntry == null)
    //        Debug.Log("I'm empty!");
    //    else
    //        Debug.Log($"I contain {inventoryEntry.item.itemName}!  {inventoryEntry.stackSize} of them in fact!");
    //}

    public void OnPointerClick(PointerEventData eventData)
    {
        EventManager.TriggerInventorySlotClickedEvent(this);
    }
}
