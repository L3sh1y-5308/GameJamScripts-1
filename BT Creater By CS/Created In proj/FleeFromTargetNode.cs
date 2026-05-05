using UnityEngine;
using UnityEngine.AI;

public class FleeFromTargetNode : Node
{
    private NavMeshAgent agent;
    private Transform threatTarget;
    private float fleeDistance;
    private float speed;

    public FleeFromTargetNode(NavMeshAgent agent, Transform threatTarget, float fleeDistance = 20f, float speed = 5f)
    {
        this.agent = agent;
        this.threatTarget = threatTarget;
        this.fleeDistance = fleeDistance;
        this.speed = speed;
    }

    public override NodeState Evaluate()
    {
        if (threatTarget == null || !agent.isOnNavMesh)
        {
            state = NodeState.Failure;
            return state;
        }

        Vector3 fleeDirection = (agent.transform.position - threatTarget.position).normalized;
        Vector3 fleePosition = agent.transform.position + fleeDirection * fleeDistance;

        if (NavMesh.SamplePosition(fleePosition, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            agent.speed = speed;
            state = NodeState.Running;
            return state;
        }

        state = NodeState.Failure;
        return state;
    }

    public void SetThreat(Transform newThreat) => threatTarget = newThreat;
}