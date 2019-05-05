using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ActionVariable("Animation", ActionVariableType.String)]
public class AnimationAction : Action
{
    AnimatorStateInfo info;

    public override void OnActivate()
    {
        string animation = ActionState.Get<string>("Animation");
        ActionState.targetActor.PlayAnimation(animation);
        if (!ActionState.yielded)
            ActionState.Complete();
    }

    public override void OnTick()
    {
        string animation = ActionState.Get<string>("Animation");
        info = ActionState.targetActor.GetAnimatorStateInfo();
        if (info.IsName(animation))
        {
            if (info.normalizedTime >= 1f || !info.loop)
                ActionState.Complete();
        }
    }
}

[ActionVariable("Variable Test", ActionVariableType.VariableTest)]
[ActionVariable("Failed Sequence", ActionVariableType.Sequence)]
[ActionVariable("Success Sequence", ActionVariableType.Sequence)]
public class VariableBranchAction : Action
{
    public override void OnActivate()
    {
        VariableTest test = ActionState.Get<VariableTest>("Variable Test");
        Sequence failedBranch = ActionState.Get<Sequence>("Failed Sequence");
        Sequence successBranch = ActionState.Get<Sequence>("Success Sequence");

        bool isTrue = GameInstance.Singleton.VariableManager.IsVariableTrue(test);
        Sequence branch = isTrue ? successBranch : failedBranch;

        Debug.Log("BranchAction isTrue: " + isTrue + " branch " + branch.name);
        if (branch != null)
            GameInstance.Singleton.SequenceManager.ActivateSequence(branch);

        ActionState.Complete();
    }
}

[ActionVariable("Tested Item", ActionVariableType.Item)]
[ActionVariable("Failed Sequence", ActionVariableType.Sequence)]
[ActionVariable("Success Sequence", ActionVariableType.Sequence)]
public class ItemBranchAction : Action
{
    public override void OnActivate()
    {
        Item item = ActionState.Get<Item>("Tested Item");
        Sequence failedBranch = ActionState.Get<Sequence>("Failed Sequence");
        Sequence successBranch = ActionState.Get<Sequence>("Success Sequence");

        Sequence branch = (Sequence.interactorCurrentItem == item) ? successBranch : failedBranch;
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
        Item item = ActionState.Get<Item>("Given Item");
        ActionState.targetActor.AddItem(item);
        ActionState.Complete();
    }
}

[ActionVariable("Removed Item", ActionVariableType.Item)]
public class RemoveItemAction : Action
{
    public override void OnActivate()
    {
        Item item = ActionState.Get<Item>("Removed Item");
        ActionState.targetActor.RemoveItem(item);
        ActionState.Complete();
    }
}

[ActionVariable("Walk Target", ActionVariableType.String)]
[ActionVariable("Position Target", ActionVariableType.Vector2)]
public class WalkAction : Action
{
    public override void OnActivate()
    {
        ActionState.targetActor.onArrivedLocation += OnArrivedLocation;

        string walkTarget = ActionState.Get<string>("Walk Target");
        Vector2 position = ActionState.Get<Vector2>("Position Target");

        if (walkTarget != string.Empty)
        {
            Actor actor = Actor.GetActorByName(walkTarget);
            if (actor != null)
                ActionState.targetActor.MoveToLocation(actor.transform.position);
        }
        else
            ActionState.targetActor.MoveToLocation(position);

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

[ActionVariable("Burst Text", ActionVariableType.String)]
public class BurstTextAction : Action
{
    public override void OnActivate()
    {
        string burstText = ActionState.Get<string>("Burst Text");
        BurstText.Spawn(BurstTextStyle.Info, ActionState.targetActor.transform, burstText);
        ActionState.Complete();
    }
}