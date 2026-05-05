using System;

public class FunctionalNode : Node
{
    private Func<NodeState> function;

    public FunctionalNode(Func<NodeState> function)
    {
        this.function = function;
    }

    public override NodeState Evaluate()
    {
        state = function();
        return state;
    }
}