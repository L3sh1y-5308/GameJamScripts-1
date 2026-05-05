using System.Collections.Generic;
using UnityEngine;

// Состояния, в которых может находиться узел
public enum NodeState { Running, Success, Failure }

// Базовый класс для всех узлов
public abstract class Node
{
    public NodeState state;
    public abstract NodeState Evaluate();
}

// Sequence: выполняет дочерние узлы по очереди. Проваливается, если провалится хотя бы один.
public class Sequence : Node
{
    protected List<Node> nodes = new List<Node>();

    public Sequence(List<Node> nodes) { this.nodes = nodes; }

    public override NodeState Evaluate()
    {
        bool anyChildIsRunning = false;
        foreach (Node node in nodes)
        {
            switch (node.Evaluate())
            {
                case NodeState.Failure:
                    state = NodeState.Failure;
                    return state;
                case NodeState.Success:
                    continue;
                case NodeState.Running:
                    anyChildIsRunning = true;
                    continue;
            }
        }
        state = anyChildIsRunning ? NodeState.Running : NodeState.Success;
        return state;
    }
}

// Selector: выполняет дочерние узлы. Успешен, если успешен хотя бы один.
public class Selector : Node
{
    protected List<Node> nodes = new List<Node>();

    public Selector(List<Node> nodes) { this.nodes = nodes; }

    public override NodeState Evaluate()
    {
        foreach (Node node in nodes)
        {
            switch (node.Evaluate())
            {
                case NodeState.Failure:
                    continue;
                case NodeState.Success:
                    state = NodeState.Success;
                    return state;
                case NodeState.Running:
                    state = NodeState.Running;
                    return state;
            }
        }
        state = NodeState.Failure;
        return state;
    }
}