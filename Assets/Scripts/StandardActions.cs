using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugAction : Action
{
    float timeout = 0;

    public override void OnActivate(Sequence sequence, ActionState state)
    {
        timeout = state.time + Time.time;
        Debug.Log("OnActivate: " + state.targetActor);
    }

    public override void OnTick(Sequence sequence, ActionState state)
    {
        if (timeout < Time.time)
        {
            Debug.Log("Complete: " + state.targetActor);
            state.Complete(sequence);
        }
    }
}

public class BranchAction : Action
{
    public override void OnActivate(Sequence sequence, ActionState state)
    {
        bool isTrue = GameInstance.Singleton.variableManager.IsVariableTrue(state.branch.test);
        Sequence branch = isTrue ? state.branch.success : state.branch.failure;

        Debug.Log("BranchAction isTrue: " + isTrue + " branch " + branch.name);
        if (branch != null)
            GameInstance.Singleton.sequenceManager.ActivateSequence(branch);

        state.Complete(sequence);
    }
}

public class WalkAction : Action
{
    ActionState currentState; // todo: rewrite actions to work with global vars like this.
    Sequence currentSequence;

    public override void OnActivate(Sequence sequence, ActionState state)
    {
        currentSequence = sequence;
        currentState = state;

        state.targetActor.onArrivedLocation += OnArrivedLocation;
        state.targetActor.MoveToLocation(state.position);
    }

    void OnArrivedLocation(Actor actor)
    {
        currentState.Complete(currentSequence);
    }
}

public class BurstTextAction : Action
{
    public override void OnActivate(Sequence sequence, ActionState state)
    {
        BurstText.Spawn(BurstTextStyle.Info, state.targetActor.transform, state.text);
        state.Complete(sequence);
    }
}