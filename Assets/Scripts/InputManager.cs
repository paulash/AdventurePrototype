using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Sequence walkSequence;
    Actor controlledActor;

    IInteractable interactionTarget = null;
    SequenceInstance sequenceInstance = null;

    // Start is called before the first frame update
    void Start()
    {
        GameInstance.Singleton.onRoomChange += OnRoomChange;
        OnRoomChange(null, GameInstance.Singleton.ControlledActor);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !SteamPunkConsole.IsConsoleOpen && 
            !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            Vector3 targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // if clicked on a thing
            // do, interact sequence

            Collider2D hitCollider = Physics2D.OverlapPoint(targetPosition, LayerMask.GetMask("Interactable"));
            if (hitCollider)
            {
                // determine where to walk to interact. (interaction distance stored in target?)
                // update the walk sequence (abort it if its already running) this is the only time we need to 'cancel' a sequence.
                // when we've arrived, call 'DoInteract' on target.
                interactionTarget = hitCollider.GetComponent<IInteractable>();
                if (interactionTarget != null)
                {
                    if (interactionTarget.CanInteract(controlledActor))
                    {
                        if (sequenceInstance != null)
                            GameInstance.Singleton.SequenceManager.AbortSequence(sequenceInstance);

                        walkSequence.actionStates[0].Set("Position Target", (Vector2)interactionTarget.GetInteractionPosition(controlledActor));
                        sequenceInstance = GameInstance.Singleton.SequenceManager.ActivateSequence(walkSequence);
                    }
                }
            }
            else
            {
                interactionTarget = null;
                targetPosition.z = 0;

                walkSequence.actionStates[0].Set("Position Target", (Vector2)targetPosition);
                if (sequenceInstance != null)
                    GameInstance.Singleton.SequenceManager.AbortSequence(sequenceInstance);

                sequenceInstance = GameInstance.Singleton.SequenceManager.ActivateSequence(walkSequence);
            }
        }
    }

    void OnWalkComplete(ActionState walkState)
    {
        if (interactionTarget != null)
            interactionTarget.DoInteract(controlledActor);

        interactionTarget = null;
        sequenceInstance = null;
    }

    public void OnRoomChange(Room room, Actor newControlledActor)
    {
        bool newActor = newControlledActor != controlledActor;

        if (controlledActor != null && newActor)
            walkSequence.actionStates[0].onActionComplete -= OnWalkComplete;

        controlledActor = newControlledActor;
        if (controlledActor != null && newActor)
        {
            walkSequence.actionStates[0].targetActorName = controlledActor.name;
            walkSequence.actionStates[0].onActionComplete += OnWalkComplete;
        }
    }

}
