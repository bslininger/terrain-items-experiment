using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUIController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemSpriteImage;
    [SerializeField] private TextMeshProUGUI stackText;
    private bool hasStack;
    private int? cachedStackValue = null;

    public IInputLockProvider InputLockProvider { get; set; }
    public enum ClickType: int
    {
        Regular,
        Shift,
        Ctrl
    }
    public ClickType SlotClickType { get; private set; }

    public void SetSlot(InventorySlotDisplayInformation displayInformation)
    {
        if (displayInformation.HasItem)
        {
            itemSpriteImage.sprite = displayInformation.Icon;
            stackText.text = displayInformation.StackSize.ToString();
            SetStackExistenceAndTextVisibility(displayInformation.StackSize);
            itemSpriteImage.gameObject.SetActive(true);
        }
        else
        {
            itemSpriteImage.sprite = null;
            stackText.text = "0";
            SetStackExistenceAndTextVisibility(0);
            itemSpriteImage.gameObject.SetActive(false);
        }
    }

    public void OverrideStackText(int overrideValue)
    {
        // Used in situations such as when selecting an amount to pull from a stack where the preview slot shows the amount to be pulled in the stack text area.
        if (int.TryParse(stackText.text, out int value))
        {
            cachedStackValue = value;
            stackText.text = overrideValue.ToString();
            SetStackExistenceAndTextVisibility(overrideValue);
        }
    }

    public void ResetStackText()
    {
        if (cachedStackValue.HasValue)
        {
            stackText.text = cachedStackValue.Value.ToString();
            SetStackExistenceAndTextVisibility(cachedStackValue.Value);
        }
        else
            stackText.text = string.Empty;

        cachedStackValue = null;
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
        if (pressingShift && hasStack)
        {
            SlotClickType = ClickType.Shift;
        }
        else if (pressingCtrl && hasStack)
        {
            SlotClickType = ClickType.Ctrl;
        }
        else
        {
            SlotClickType = ClickType.Regular;
        }

        EventManager.TriggerInventorySlotClickedEvent(this);
    }

    private void SetStackExistenceAndTextVisibility(int stackSize)
    {
        // Only show the stack value on the corner of the icon if the stack has more than 1 in it.
        hasStack = stackSize > 1;
        stackText.gameObject.SetActive(hasStack);
    }
}
