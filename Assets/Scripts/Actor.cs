using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnArrivedLocation(Actor actor);
public delegate void OnItemAdded(Item item);
public delegate void OnItemRemoved(Item item);
public delegate void OnItemEquipped(Item item);

public class Actor : MonoBehaviour, IInteractable
{
    static List<Actor> activeActors = new List<Actor>();

    public OnArrivedLocation onArrivedLocation;
    public OnItemAdded onItemAdded;
    public OnItemRemoved onItemRemoved;
    public OnItemEquipped onItemEquipped;

    public Sequence interactSequence;

    Animator animator;
    SpriteRenderer spriteRenderer;

    public float arrivalDistance = 1f;
    Vector3 dest;
    bool pathing = false;
    bool faceMovement = false;

    List<Item> inventory = new List<Item>();

    public Item[] initialItems;
    public List<Item> Inventory { get { return inventory; } }

    public bool FlipX
    {
        get { return spriteRenderer.flipX; }
        set { spriteRenderer.flipX = value; }
    }

    public Item EquippedItem { get; private set; }

    public static Actor GetActorByName(string name)
    {
        foreach (Actor actor in activeActors)
        {
            if (actor.name.ToLower() == name.ToLower())
                return actor;
        }

        return null;
    }

    public void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        activeActors.Add(this);

        for (int i=0; i < initialItems.Length; i++)
            inventory.Add(initialItems[i]);

        if (inventory.Count != 0)
            EquipItem(inventory[0]);
    }

    public void Possess()
    {
        if (onItemEquipped != null)
            onItemEquipped(EquippedItem);
    }

    public void OnDestroy()
    {
        activeActors.Remove(this);
    }

    private void Update()
    {
        if (pathing)
        {
            Vector3 vec = (dest - transform.position);
            if (vec.magnitude > arrivalDistance)
            {
                if (vec.x != 0)
                    FlipX = vec.x < 0;

                transform.Translate(vec.normalized * Time.deltaTime * 4);
            }
            else
            {
                pathing = false;
                if (onArrivedLocation != null)
                    onArrivedLocation(this);
            }
        }
    }

    public void MoveToLocation(Vector3 location, bool faceMovement=true)
    {
        Vector3 vec = (location - transform.position);
        if (vec.magnitude < arrivalDistance)
        {
            if (onArrivedLocation != null)
                onArrivedLocation(this);

            return;
        }

        dest = location;
        pathing = true;
        this.faceMovement = faceMovement;
    }

    public void PlayAnimation(string animation)
    {
        animator.Play(animation);
    }

    public AnimatorStateInfo GetAnimatorStateInfo()
    {
        return animator.GetCurrentAnimatorStateInfo(0);
    }

    public bool EquipItem(Item item)
    {
        if (!HasItem(item))
            return false;

        EquippedItem = item;
        if (onItemEquipped != null)
            onItemEquipped(item);

        return true;
    }

    public bool AddItem(Item item)
    {
        if (!HasItem(item))
        {
            inventory.Add(item);
            if (onItemAdded != null)
                onItemAdded(item);

            return true;
        }
        return false;
    }

    public bool RemoveItem(Item itemName)
    {
        if (!HasItem(itemName))
            return false;

        int itemIndex = GetItemIndex(itemName);

        inventory.Remove(itemName);
        if (onItemRemoved != null)
            onItemRemoved(itemName);

        if (EquippedItem == itemName)
            EquipItem(inventory[0]);

        return true;
    }

    public int GetItemIndex(Item itemName)
    {
        return inventory.IndexOf(itemName);
    }

    public bool HasItem(Item itemName)
    {
        return GetItemIndex(itemName) != -1;
    }

    public bool CanInteract(Actor interactor)
    {
        return interactSequence != null;
    }

    public void DoInteract(Actor interactor)
    {
        if (interactSequence != null)
             GameInstance.Singleton.SequenceManager.ActivateSequence(interactSequence);
    }

    public Vector2 GetInteractionPosition(Actor interactor)
    {
        return transform.position;
    }
}
