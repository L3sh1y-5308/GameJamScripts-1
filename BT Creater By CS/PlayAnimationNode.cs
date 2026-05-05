using UnityEngine;

// Узел для проигрывания анимации
public class PlayAnimationNode : Node
{
    private Animator animator;
    private string animationStateName;

    public PlayAnimationNode(Animator animator, string animationStateName)
    {
        this.animator = animator;
        this.animationStateName = animationStateName;
    }

    public override NodeState Evaluate()
    {
        animator.Play(animationStateName);
        state = NodeState.Success;
        return state;
    }
}

// Узел для перемещения к цели
public class MoveToTargetNode : Node
{
    private Transform transform;
    private Transform target;
    private float speed;
    private float stoppingDistance;

    public MoveToTargetNode(Transform transform, Transform target, float speed, float stoppingDistance = 0.1f)
    {
        this.transform = transform;
        this.target = target;
        this.speed = speed;
        this.stoppingDistance = stoppingDistance;
    }

    public override NodeState Evaluate()
    {
        if (target == null) return NodeState.Failure;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > stoppingDistance)
        {
            // Двигаемся к цели
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            transform.LookAt(target);
            state = NodeState.Running;
            return state;
        }

        // Достигли цели
        state = NodeState.Success;
        return state;
    }
}