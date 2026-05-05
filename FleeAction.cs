using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FleeAction", story: "[Self] flees from [EnemyReunOf] using [NavMeshAgent]", category: "Action", id: "d0827144ce67c3e0619f56e7fe15f93f")]
public partial class FleeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<string> EnemyReunOf;
    [SerializeReference] public BlackboardVariable<NavMeshAgent> NavMeshAgent;
    [SerializeReference] public BlackboardVariable<float> FleeDistance = new BlackboardVariable<float>(5f);
    [SerializeReference] public BlackboardVariable<float> SafeDistance = new BlackboardVariable<float>(15f);

    protected override Status OnStart()
    {
        if (Self.Value == null || NavMeshAgent.Value == null) 
            return Status.Failure;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self.Value == null || NavMeshAgent.Value == null) 
            return Status.Failure;

        GameObject enemy = GameObject.Find(EnemyReunOf.Value);
        if (enemy == null) 
            return Status.Failure;

        float distanceToEnemy = Vector3.Distance(Self.Value.transform.position, enemy.transform.position);

        if (distanceToEnemy >= SafeDistance.Value)
            return Status.Success;

        Vector3 fleeDirection = Self.Value.transform.position - enemy.transform.position;
        Vector3 fleeTargetPoint = Self.Value.transform.position + fleeDirection.normalized * FleeDistance.Value;

        if (NavMeshAgent.Value.isOnNavMesh)
            NavMeshAgent.Value.SetDestination(fleeTargetPoint);

        return Status.Running;
    }
}

