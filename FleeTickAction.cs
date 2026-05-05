using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FleeTick", story: "[Flee] runs from [Threat] result in [IsCornered]", category: "Action", id: "335e81021ed5b5556133f067c2f6c1ee")]
public partial class FleeTickAction : Action
{
    [SerializeReference] public BlackboardVariable<FleeBehavior> Flee;
    [SerializeReference] public BlackboardVariable<GameObject> Threat;
    [SerializeReference] public BlackboardVariable<bool> IsCornered;

    protected override Status OnStart() => Status.Running;

    protected override Status OnUpdate()
    {
        if (Flee.Value == null || Threat.Value == null) return Status.Failure;

        Flee.Value.Tick(Threat.Value.transform);
        IsCornered.Value = Flee.Value.IsCornered();

        return Status.Running; // тикаем непрерывно, пока Danger не пропадёт
    }

    protected override void OnEnd()
    {
        if (Flee.Value != null) Flee.Value.ResetPanic();
    }
}

