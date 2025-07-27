using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUIController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemSpriteImage;
    [SerializeField] private TextMeshProUGUI stackText;
    private Inventory.InventoryEntry inventoryEntry;

    public IInputLockProvider InputLockProvider { get; set; }
    public enum ClickType: int
    {
        Regular,
        Shift,
        Ctrl
    }
    public ClickType SlotClickType { get; private set; }

    private bool HasStack()
    {
        return (inventoryEntry?.item?.IsStackable ?? false) && inventoryEntry.stackSize > 1;
    }

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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (InputLockProvider.InputLocked(UIInputLock.InventoryInteraction))
        {
            return;
        }

        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        bool pressingShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool pressingCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        if (pressingShift && HasStack())
        {
            SlotClickType = ClickType.Shift;
        }
        else if (pressingCtrl && HasStack())
        {
            SlotClickType = ClickType.Ctrl;
        }
        else
        {
            SlotClickType = ClickType.Regular;
        }

        EventManager.TriggerInventorySlotClickedEvent(this);
    }
}
