using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    Vector2 GetInteractionPosition(Actor interactor);
    bool CanInteract(Actor interactor);
    void DoInteract(Actor interactor);
}
