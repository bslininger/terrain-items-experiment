using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemCursorFollower : MonoBehaviour
{
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private GameObject slotPrefab;
    private RectTransform rectTransform;
    private GameObject slotUIElement;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        // The cursor follower stays enabled all the time; it's the slotUIElement that is created and destroyed when items come and go from the cursor.
        EventManager.InventoryUpdateEvent += UpdateCursorItem;
    }

    private void OnDisable()
    {
        EventManager.InventoryUpdateEvent -= UpdateCursorItem;
    }

    void Update()
    {
        if (slotUIElement == null)
            return;

        // Convert screen point to canvas-local position
        Vector2 localMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            Input.mousePosition,
            null,  // Using Screen Space - Overlay
            out localMousePosition
        );

        rectTransform.anchoredPosition = localMousePosition + new Vector2(rectTransform.rect.width / 2, -rectTransform.rect.height / 2);
    }

    private void UpdateCursorItem(int[] indicesToUpdate)
    {
        if (!Array.Exists(indicesToUpdate, element => element == Inventory.CursorSlotIndex))
        {
            return;
        }
        // Called when notified that the item on the cursor has changed
        InventorySlotDisplayInformation cursorSlotDisplayInformation = playerInventory.GetSlotDisplayInformation(Inventory.CursorSlotIndex);
        if (!cursorSlotDisplayInformation.HasItem)
        {
            Destroy(slotUIElement);
            slotUIElement = null;
        }
        else
        {
            if (slotUIElement == null)
            {
                slotUIElement = Instantiate(slotPrefab, transform);
            }
            // Set the RectTransform width and height to 72 x 72 to match other inventory slots, and the local position to x = 32, y = 32 to sit at the center of the parent container (which is 64 x 64)
            RectTransform rectTransform = slotUIElement.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(72, 72);
            rectTransform.anchoredPosition = new Vector2(32, 32);
            // The background image for a slot shouldn't show up on the cursor, and regardless, it shouldn't be clickable.
            Image slotBackgroundImage = slotUIElement.GetComponent<Image>();
            slotBackgroundImage.raycastTarget = false;
            slotBackgroundImage.enabled = false;
            InventorySlotUIController slotUIController = slotUIElement.transform.GetComponent<InventorySlotUIController>();
            slotUIController.InputLockProvider = UIManager.Instance;
            slotUIController.SetSlot(cursorSlotDisplayInformation);
        }
    }
}
