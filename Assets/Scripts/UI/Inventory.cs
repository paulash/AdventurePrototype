using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public struct ItemRef
{
    public Item item;
    public GameObject panel;
}

public class Inventory : MonoBehaviour
{
    public Image equippedItemImage;
    public GameObject inventoryPanel;
    public GameObject itemPrefab;

    Actor controlledActor;

    List<ItemRef> itemRefs = new List<ItemRef>();

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
        if (controlledActor != null)
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
        GameObject itemPanel = GameObject.Instantiate(itemPrefab, inventoryPanel.transform);
        itemPanel.GetComponent<Button>().onClick.AddListener(() => OnSelectItem(item));
        itemPanel.GetComponent<Image>().sprite = item.icon;

        itemRefs.Add(new ItemRef() { item = item, panel = itemPanel });
    }

    void OnItemRemoved(Item item)
    {

    }

    void OnItemEquipped(Item item)
    {
        equippedItemImage.sprite = item.icon;
        inventoryPanel.SetActive(false);
    }
}
