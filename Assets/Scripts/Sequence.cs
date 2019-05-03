using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnActionComplete(ActionState state);

public class Action
{
    public virtual void OnActivate(Sequence sequence, ActionState state) { }
    public virtual void OnDeactivate(Sequence sequence, ActionState state) { }
    public virtual void OnTick(Sequence sequence, ActionState state) { }
}

[System.Serializable]
public struct SequenceBranch
{
    public VariableTest test;
    public Sequence success;
    public Sequence failure;
}

[System.Serializable]
public class ActionState
{
    public string actionClass;
    public string targetActorName;
    public string targetActorAnimation;

    public Vector3 position;
    public string text;

    public float time;
    public SequenceBranch branch;

    public bool yielded = false;

    [System.NonSerialized]
    public OnActionComplete onActionComplete;

    [System.NonSerialized]
    Action action = null;

    public Actor targetActor { get; private set; }

    public void Activate(Sequence sequence)
    {
        targetActor = Actor.GetActorByName(targetActorName);
        if (targetActor != null && targetActorAnimation != "")
            targetActor.PlayAnimation(targetActorAnimation);

        action = (Action)System.Activator.CreateInstance(System.Type.GetType(actionClass));
        if (action == null)
        {
            Debug.LogError("Invalid action class: " + actionClass);
            Complete(sequence);
            return;
        }

        action.OnActivate(sequence, this);
    }

    void Deactivate(Sequence sequence)
    {
        if (action == null)
        {
            Debug.LogError("Invalid action class: " + actionClass);
            return;
        }

        action.OnDeactivate(sequence, this);
    }

    public void Tick(Sequence sequence)
    {
        if (action == null)
        {
            Debug.LogError("Invalid action class: " + actionClass);
            Complete(sequence);
            return;
        }

        action.OnTick(sequence, this);
    }

    public void Complete(Sequence sequence)
    {
        if (onActionComplete != null)
            onActionComplete(this);

        Deactivate(sequence);
    }
}

public delegate void OnSequenceComplete(Sequence sequence);

[CreateAssetMenu(fileName = "NewSequence", menuName = "Sequence", order = 1)]
public class Sequence : ScriptableObject
{
    public OnSequenceComplete onSequenceComplete;
    public ActionState[] actionStates;

    [System.NonSerialized]
    ActionState yieldedAction;

    [System.NonSerialized]
    int currentActionState = 0;

    [System.NonSerialized]
    List<ActionState> activeActionStates = new List<ActionState>(32);

    public void Start()
    {
        currentActionState = 0;
        NextAction();
    }

    void NextAction()
    {
        if (actionStates.Length > currentActionState)
        {
            actionStates[currentActionState].Activate(this);
            actionStates[currentActionState].onActionComplete += OnActionComplete;

            activeActionStates.Add(actionStates[currentActionState]);
            yieldedAction = actionStates[currentActionState].yielded ? actionStates[currentActionState] : null;

            currentActionState++;

            if (yieldedAction == null)
                NextAction();
        }
    }

    void OnActionComplete(ActionState actionState)
    {
        int index = activeActionStates.IndexOf(actionState);
        if (index != -1)
        {
            activeActionStates.RemoveAt(index);
            if (actionState == yieldedAction)
            {
                NextAction();
                yieldedAction = null;
            }
        }

        if (activeActionStates.Count == 0)
            Complete();
    }

    void Complete()
    {
        if (onSequenceComplete != null)
            onSequenceComplete(this);
    }

    public void Tick()
    {
        for (int i=0; i < activeActionStates.Count; i++)
            activeActionStates[i].Tick(this);
    }

}
