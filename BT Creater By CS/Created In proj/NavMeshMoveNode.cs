using UnityEngine;
using UnityEngine.AI;

public class NavMeshMoveNode : Node
{
    private NavMeshAgent agent;
    private Transform target;
    private float stoppingDistance;

    public NavMeshMoveNode(NavMeshAgent agent, Transform target, float stoppingDistance = 0.5f)
    {
        this.agent = agent;
        this.target = target;
        this.stoppingDistance = stoppingDistance;
    }

    public override NodeState Evaluate()
    {
        if (target == null || !agent.isOnNavMesh)
        {
            state = NodeState.Failure;
            return state;
        }

        agent.SetDestination(target.position);
        agent.stoppingDistance = stoppingDistance;

        float distance = Vector3.Distance(agent.transform.position, target.position);
        if (distance <= stoppingDistance && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f))
        {
            state = NodeState.Success;
            return state;
        }

        state = NodeState.Running;
        return state;
    }

    public void SetTarget(Transform newTarget) => target = newTarget;
}