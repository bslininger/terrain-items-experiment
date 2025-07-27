using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StackSizeSelectorPanelController : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private InventorySlotUIController slotUIController;
    private Inventory.InventoryEntry inventoryEntry;

    private Action<int> onAccept; // This is the callback for the Accept button, defined outside this controller but stored inside it. Destroying afterward handled outside as well.
    private Action onCancel;
    private int currentSelectedAmount => Mathf.RoundToInt(slider.value);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputField.text = "1";

        slider.onValueChanged.AddListener((float value) =>
        {
            inputField.text = Mathf.RoundToInt(value).ToString();
        });
        slider.minValue = 1;
        slider.wholeNumbers = true;

        inputField.onValueChanged.AddListener((string text) =>
        {
            if (int.TryParse(text, out int value))
            {
                value = Mathf.Clamp(value, 1, (int)slider.maxValue);
                slider.value = value;
                inventoryEntry.SetStackSize(value);
                slotUIController.SetSlot(inventoryEntry);
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetInventoryEntry(Inventory.InventoryEntry entry)
    {
        inventoryEntry = entry;
        slider.maxValue = inventoryEntry.stackSize;
        slider.value = 1;
        entry.SetStackSize(1);
        slotUIController.SetSlot(inventoryEntry);
    }

    public void SetAcceptAction(Action<int> callback)
    {
        onAccept = callback;
    }

    public void SetCancelAction(Action callback)
    {
        onCancel = callback;
    }

    public void OnAcceptButtonClicked()
    {
        onAccept?.Invoke(currentSelectedAmount);
    }

    public void OnCancelButtonClicked()
    {
        onCancel?.Invoke();
    }

    public void IncrementValue()
    {
        slider.value += 1;
    }

    public void DecrementValue()
    {
        slider.value -= 1;
    }
}
