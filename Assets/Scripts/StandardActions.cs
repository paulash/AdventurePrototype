using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ActionVariable("Animation", VariableType.String)]
public class AnimationAction : Action
{
    AnimatorStateInfo info;

    public override void OnActivate()
    {
        string animation = State.Get<string>("Animation");
        State.targetActor.PlayAnimation(animation);
        if (!State.yielded)
            State.Complete();
    }

    public override void OnTick()
    {
        string animation = State.Get<string>("Animation");
        info = State.targetActor.GetAnimatorStateInfo();
        if (info.IsName(animation))
        {
            if (info.normalizedTime >= 1f || !info.loop)
                State.Complete();
        }
    }
}

/*
[ActionVariable("Variable Test", ActionVariableType.VariableTest)]
[ActionVariable("Success Sequence", ActionVariableType.Sequence)]
[ActionVariable("Failed Sequence", ActionVariableType.Sequence)]
public class VariableBranchAction : Action
{
    public override void OnActivate()
    {
        VariableTest test = ActionState.Get<VariableTest>("Variable Test");
        Sequence failedBranch = ActionState.Get<Sequence>("Failed Sequence");
        Sequence successBranch = ActionState.Get<Sequence>("Success Sequence");

        bool isTrue = GameInstance.Singleton.VariableManager.VariableType(test);
        Sequence branch = isTrue ? successBranch : failedBranch;

        Debug.Log("BranchAction isTrue: " + isTrue + " branch " + branch.name);
        if (branch != null)
            GameInstance.Singleton.SequenceManager.ActivateSequence(branch);

        ActionState.Complete();
    }
}
*/

[ActionVariable("Tested Item", VariableType.Item)]
[ActionVariable("Success Sequence", VariableType.Sequence)]
[ActionVariable("Failed Sequence", VariableType.Sequence)]
public class EquipItemBranchAction : Action
{
    public override void OnActivate()
    {
        Item item = State.Get<Item>("Tested Item");
        Sequence failedBranch = State.Get<Sequence>("Failed Sequence");
        Sequence successBranch = State.Get<Sequence>("Success Sequence");

        Sequence branch = (State.targetActor.EquippedItem == item) ? successBranch : failedBranch;
        if (branch != null)
            GameInstance.Singleton.SequenceManager.ActivateSequence(branch);

        State.Complete();
    }
}

[ActionVariable("Tested Item", VariableType.Item)]
[ActionVariable("Success Sequence", VariableType.Sequence)]
[ActionVariable("Failed Sequence", VariableType.Sequence)]
public class HasItemBranchAction : Action
{
    public override void OnActivate()
    {
        Item item = State.Get<Item>("Tested Item");
        Sequence failedBranch = State.Get<Sequence>("Failed Sequence");
        Sequence successBranch = State.Get<Sequence>("Success Sequence");

        Sequence branch = State.targetActor.HasItem(item) ? successBranch : failedBranch;
        if (branch != null)
            GameInstance.Singleton.SequenceManager.ActivateSequence(branch);

        State.Complete();
    }
}

[ActionVariable("Equip Item", VariableType.Item)]
public class EquipItemAction : Action
{
    public override void OnActivate()
    {
        Item item = State.Get<Item>("Equip Item");
        State.targetActor.EquipItem(item);
        State.Complete();
    }
}


[ActionVariable("Given Item", VariableType.Item)]
public class AddItemAction : Action
{
    public override void OnActivate()
    {
        Item item = State.Get<Item>("Given Item");
        State.targetActor.AddItem(item);
        State.Complete();
    }
}

[ActionVariable("Removed Item", VariableType.Item)]
public class RemoveItemAction : Action
{
    public override void OnActivate()
    {
        Item item = State.Get<Item>("Removed Item");
        State.targetActor.RemoveItem(item);
        State.Complete();
    }
}


[ActionVariable("Walk Target", VariableType.String)]
[ActionVariable("Position Target", VariableType.Vector2)]
public class WalkAction : Action
{
    public override void OnActivate()
    {
        State.targetActor.onArrivedLocation += OnArrivedLocation;

        string walkTarget = State.Get<string>("Walk Target");
        Vector2 position = State.Get<Vector2>("Position Target");

        if (walkTarget != string.Empty)
        {
            Actor actor = Actor.GetActorByName(walkTarget);
            if (actor != null)
                State.targetActor.MoveToLocation(actor.transform.position);
        }
        else
            State.targetActor.MoveToLocation(position);

        State.targetActor.PlayAnimation("walk");
    }

    public override void OnDeactivate()
    {
        State.targetActor.PlayAnimation("idle");
    }

    void OnArrivedLocation(Actor actor)
    {
        State.Complete();
    }
}


[ActionVariable("Burst Text", VariableType.String)]
public class BurstTextAction : Action
{
    public override void OnActivate()
    {
        string burstText = State.Get<string>("Burst Text");
        BurstText.Spawn(BurstTextStyle.Info, State.targetActor.transform, burstText);
        State.Complete();
    }
}