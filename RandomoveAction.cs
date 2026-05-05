using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Randomove", story: "[Self] moves to [RandomMoves] Points for [Duration] sec", category: "Action", id: "ab3405f66f7416581ee68f3870e354aa")]
public partial class RandomoveAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<RandomMovement> RandomMoves;
    [SerializeReference] public BlackboardVariable<float> Duration;

    private float elapsedTime;

    protected override Status OnStart()
    {
        if (RandomMoves.Value == null) return Status.Failure;

        elapsedTime = 0f;
        RandomMoves.Value.enabled = true;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (RandomMoves.Value == null) return Status.Failure;

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= Duration.Value)
            return Status.Success; // Время вышло — переходим дальше в Sequence

        return Status.Running; // Продолжаем бродить
    }

    protected override void OnEnd()
    {
        if (RandomMoves.Value != null)
            RandomMoves.Value.enabled = false;
    }
}