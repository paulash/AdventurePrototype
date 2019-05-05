using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ActionVariable("Time", ActionVariableType.Float)]
public class DebugAction : Action
{
    float timeout = 0;

    public override void OnActivate()
    {
        timeout = ActionState.Get<float>("Time") + Time.time;
        Debug.Log("OnActivate: " + ActionState.text);
    }

    public override void OnTick()
    {
        if (timeout < Time.time)
        {
            Debug.Log("Complete: " + ActionState.text);
            ActionState.Complete();
        }
    }
}

public class AnimationAction : Action
{
    AnimatorStateInfo info;

    public override void OnActivate()
    {
        ActionState.targetActor.PlayAnimation(ActionState.text);
        if (!ActionState.yielded)
            ActionState.Complete();
    }

    public override void OnTick()
    {
        info = ActionState.targetActor.GetAnimatorStateInfo();
        if (info.IsName(ActionState.text))
        {
            if (info.normalizedTime >= 1f || !info.loop)
                ActionState.Complete();
        }
    }
}

public class VariableBranchAction : Action
{
    public override void OnActivate()
    {
        bool isTrue = GameInstance.Singleton.VariableManager.IsVariableTrue(ActionState.branch.test);
        Sequence branch = isTrue ? ActionState.branch.success : ActionState.branch.failure;

        Debug.Log("BranchAction isTrue: " + isTrue + " branch " + branch.name);
        if (branch != null)
            GameInstance.Singleton.SequenceManager.ActivateSequence(branch);

        ActionState.Complete();
    }
}

public class ItemBranchAction : Action
{
    public override void OnActivate()
    {
        Sequence branch = (Sequence.interactorCurrentItem == ActionState.item) ? ActionState.branch.success : ActionState.branch.failure;
        if (branch != null)
            GameInstance.Singleton.SequenceManager.ActivateSequence(branch);

        ActionState.Complete();
    }
}

[ActionVariable("Given Item", ActionVariableType.Item)]
public class AddItemAction : Action
{
    public override void OnActivate()
    {
        ActionState.targetActor.AddItem(ActionState.item);
        ActionState.Complete();
    }
}

public class RemoveItemAction : Action
{
    public override void OnActivate()
    {
        ActionState.targetActor.RemoveItem(ActionState.item);
        ActionState.Complete();
    }
}

public class WalkAction : Action
{
    public override void OnActivate()
    {
        ActionState.targetActor.onArrivedLocation += OnArrivedLocation;

        if (ActionState.text != string.Empty)
        {
            Actor actor = Actor.GetActorByName(ActionState.text);
            if (actor != null)
                ActionState.targetActor.MoveToLocation(actor.transform.position);
        }
        else
            ActionState.targetActor.MoveToLocation(ActionState.position);

        ActionState.targetActor.PlayAnimation("walk");
    }

    public override void OnDeactivate()
    {
        ActionState.targetActor.PlayAnimation("idle");
    }

    void OnArrivedLocation(Actor actor)
    {
        ActionState.Complete();
    }
}

public class BurstTextAction : Action
{
    public override void OnActivate()
    {
        BurstText.Spawn(BurstTextStyle.Info, ActionState.targetActor.transform, ActionState.text);
        ActionState.Complete();
    }
}