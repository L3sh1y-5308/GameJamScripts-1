using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AfterAttack", story: "[Feeder] checks if food died and restores hunger", category: "Action", id: "c2d3e4f5a6b70819203a4b5c6d7e8f02")]
public partial class AfterAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<FeedOnKill> Feeder;

    protected override Status OnStart()
    {
        if (Feeder.Value == null) return Status.Failure;
        Feeder.Value.AfterAttack();
        return Status.Success;
    }

    protected override Status OnUpdate() => Status.Success;
    protected override void OnEnd() { }
}