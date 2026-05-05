using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "BeforeAttack", story: "[Feeder] tracks nearby food before attack", category: "Action", id: "b1c2d3e4f5a60718293a4b5c6d7e8f01")]
public partial class BeforeAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<FeedOnKill> Feeder;

    protected override Status OnStart()
    {
        if (Feeder.Value == null) return Status.Failure;
        Feeder.Value.BeforeAttack();
        return Status.Success;
    }

    protected override Status OnUpdate() => Status.Success;
    protected override void OnEnd() { }
}