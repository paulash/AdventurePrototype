using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public delegate void OnActionComplete(ActionState state);

[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
public class ActionVariable : System.Attribute
{
    public string name;
    public ActionVariableType type;

    public ActionVariable(string name, ActionVariableType type)
    {
        this.name = name;
        this.type = type;
    }
}

[System.Serializable]
public class ActionVariablePair
{
    public string name;
    public object value;
    public ActionVariableType type;
}

public enum ActionVariableType
{
    Invalid, Vector2, Float, String, Int, Bool, Item
}

public class Action
{
    public SequenceInstance Sequence { get; private set; }
    public ActionState ActionState { get; private set; }

    public void Init(SequenceInstance sequence, ActionState actionState)
    {
        Sequence = sequence;
        ActionState = actionState;
    }

    public virtual void OnActivate() { }
    public virtual void OnDeactivate() { }
    public virtual void OnTick() { }
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

    public Vector3 position;
    public string text = string.Empty;
    public Item item = null;

    public float time;
    public SequenceBranch branch;

    public bool cancelable = false;
    public bool yielded = false;

    public List<ActionVariablePair> variables = new List<ActionVariablePair>();

    [System.NonSerialized]
    public OnActionComplete onActionComplete;

    [System.NonSerialized]
    Action action = null;

    public Actor targetActor { get; private set; }

    public ActionVariablePair GetActionVariableType(string name)
    {
        for (int i=0; i < variables.Count; i++)
        {
            if (variables[i].name == name)
                return variables[i];
        }

        return default(ActionVariablePair);
    }

    public T Get<T>(string name)
    {
        ActionVariablePair pair = GetActionVariableType(name);
        switch(pair.type)
        {
            case ActionVariableType.Item:
                if (pair.value is Item)
                    return (T)pair.value;
                break;
            case ActionVariableType.Int:
                if (pair.value is int)
                    return (T)pair.value;
                break;
            case ActionVariableType.String:
                if (pair.value is string)
                    return (T)pair.value;
                break;
            case ActionVariableType.Float:
                if (pair.value is float)
                    return (T)pair.value;
                break;
        }

        return default(T);
    }

    public void Set(string name, object value)
    {
        ActionVariablePair pair = GetActionVariableType(name);
        if (pair.type == ActionVariableType.Invalid) return;
        pair.value = value;
    }

    public void InitializeVariables()
    {
        variables.Clear();

        System.Type actionType = System.Type.GetType(actionClass);
        IEnumerable<ActionVariable> actionVars = actionType.GetCustomAttributes<ActionVariable>();
        foreach (ActionVariable var in actionVars)
        {
            variables.Add(new ActionVariablePair()
            {
                name = var.name,
                type = var.type
            });
        }
    }

    public void Activate(SequenceInstance sequence)
    {
        targetActor = Actor.GetActorByName(targetActorName);
        System.Type actionType = System.Type.GetType(actionClass);
        // validate variables?
        action = (Action)System.Activator.CreateInstance(actionType);
        if (action == null)
        {
            Debug.LogError("Invalid action class: " + actionClass);
            Complete();
            return;
        }

        action.Init(sequence, this);
        action.OnActivate();
    }

    void Deactivate()
    {
        if (action == null)
        {
            Debug.LogError("Invalid action class: " + actionClass);
            return;
        }

        action.OnDeactivate();
    }

    public void Tick()
    {
        if (action == null)
        {
            Debug.LogError("Invalid action class: " + actionClass);
            Complete();
            return;
        }

        action.OnTick();
    }

    public void Complete()
    {
        if (onActionComplete != null)
            onActionComplete(this);

        Deactivate();
    }
}

[CreateAssetMenu(fileName = "NewSequence", menuName = "Sequence", order = 1)]
public class Sequence : ScriptableObject
{
    public List<ActionState> actionStates;
}

public delegate void OnSequenceComplete(SequenceInstance sequence);

public class SequenceInstance
{
    public OnSequenceComplete onSequenceComplete;
    public Item interactorCurrentItem = null;

    public Sequence Sequence { get; private set; }

    ActionState yieldedAction;
    int currentActionState = 0;
    List<ActionState> activeActionStates = new List<ActionState>(32);

    public SequenceInstance(Sequence sequence)
    {
        this.Sequence = sequence;
    }

    public void Start()
    {
        currentActionState = 0;
        NextAction();
    }

    public bool IsEffectingActor(string name) // TODO: make faster with a hashset.
    {
        for (int i=0; i < activeActionStates.Count; i++)
        {
            if (activeActionStates[i].targetActorName == name)
                return true;
        }

        return false;
    }

    void NextAction()
    {
        if (Sequence.actionStates.Count > currentActionState)
        {
            Sequence.actionStates[currentActionState].Activate(this);
            Sequence.actionStates[currentActionState].onActionComplete += OnActionComplete;

            activeActionStates.Add(Sequence.actionStates[currentActionState]);
            yieldedAction = Sequence.actionStates[currentActionState].yielded ? Sequence.actionStates[currentActionState] : null;

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
            activeActionStates[i].Tick();
    }
}
