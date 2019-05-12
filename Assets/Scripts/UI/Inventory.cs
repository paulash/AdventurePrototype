using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
    public Image equippedItemImage;
    public GameObject inventoryPanel;
    public GameObject inventoryContainer;
    public GameObject itemPrefab;

    Actor controlledActor;

    Dictionary<string, GameObject> itemRefs = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        GameInstance.Singleton.onRoomChange += OnRoomChange;
        OnRoomChange(null, GameInstance.Singleton.ControlledActor);
    }

    public void OnTogglePanel()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    public void OnSelectItem(Item item)
    {
        if (controlledActor.EquippedItem == item) return;

        if (controlledActor.EquippedItem.dropSequence != null)
            GameInstance.Singleton.InputManager.TryCombined(controlledActor.EquippedItem, item);
        else
            controlledActor.EquipItem(item);
    }

    void OnRoomChange(Room room, Actor newControlledActor)
    {
        bool newActor = newControlledActor != controlledActor;
        if (controlledActor != null && newActor)
        {
            controlledActor.onItemAdded -= OnItemAdded;
            controlledActor.onItemRemoved -= OnItemRemoved;
            controlledActor.onItemEquipped -= OnItemEquipped;
        }
        controlledActor = newControlledActor;
        if (controlledActor != null && newActor)
        {
            controlledActor.onItemAdded += OnItemAdded;
            controlledActor.onItemRemoved += OnItemRemoved;
            controlledActor.onItemEquipped += OnItemEquipped;

            InitializeItems();
        }
    }

    void InitializeItems()
    {
        if (controlledActor == null) return;

        List<Item> items = controlledActor.Inventory;
        for (int i = 0; i < items.Count; i++)
            OnItemAdded(items[i]);
    }

    void OnItemAdded(Item item)
    {
        GameObject itemPanel = GameObject.Instantiate(itemPrefab, inventoryContainer.transform);
        itemPanel.GetComponent<Button>().onClick.AddListener(() => OnSelectItem(item));
        itemPanel.GetComponent<Image>().sprite = item.icon;

        itemRefs.Add(item.name, itemPanel);
    }

    void OnItemRemoved(Item item)
    {
        GameObject itemPanel = null;
        if (itemRefs.TryGetValue(item.name, out itemPanel))
        {
            GameObject.Destroy(itemPanel);
            itemRefs.Remove(item.name);
        }
    }

    void OnItemEquipped(Item item)
    {
        equippedItemImage.sprite = item.icon;
        Cursor.SetCursor(item.icon.texture, Vector2.zero, CursorMode.ForceSoftware);
    }

    void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.IsPointerMoving())
            inventoryPanel.SetActive(false);
    }
}
